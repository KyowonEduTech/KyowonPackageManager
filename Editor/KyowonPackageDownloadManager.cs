using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Diagnostics;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Linq;

namespace Kyowon.Package
{
    public class KyowonPackageDownloadManager
    {
        const string PACKAGE_LIST_API_URL = "https://api.github.com/users/KyowonEduTech/packages?package_type=npm";
        const string PACKAGE_INFO_API_URL = "https://npm.pkg.github.com/@kyowonedutech/";

        const string COMMON_PACKAGE = "com.kyowon.unityplugins.";


        public static async Task<bool> CheckToken()
        {
            return await GetPackageInfoList() != null;
        }

        public static async Task<List<KyowonPackageInfo>> GetPackageInfoList(IProgress<float> progress = null)
        {
            progress ??= new KyowonPackageProgress(KyowonPackageProgress.MSG_LOAD_LIST);

            string data = await Request<string>(PACKAGE_LIST_API_URL, progress);
            if (string.IsNullOrEmpty(data))
            {
                progress.Report(1);
                return null;
            }

            List<KyowonPackageInfo> infoList = JsonConvert.DeserializeObject<List<KyowonPackageInfo>>(data);
            foreach (var i in infoList)
            {
                string detail = await Request<string>(PACKAGE_INFO_API_URL + i.Name, progress);
                JsonConvert.PopulateObject(detail, i);
                i.DisplayName ??= i.Name.Replace(COMMON_PACKAGE, string.Empty);
            }

            progress.Report(1);
            return infoList;
        }

        public static async Task Download(KyowonPackageInfo info, string version, IProgress<float> progress = null)
        {
            string name = info.Name;
            var versionInfo = info.Versions[version];

            string url = versionInfo.URL;
            string filePath = Path.Combine(KyowonPackageManager.GetModuleRootPath(), $"{name}.tar.gz");

            byte[] data = await Request<byte[]>(url, progress);
            await File.WriteAllBytesAsync(filePath, data);

            await UnpackTarball(filePath, name);
            if (versionInfo.Dependencies != null)
            {
                await DownloadDependencies(versionInfo.Dependencies);
            }
            UnityEngine.Debug.Log($"{info.Name} Download Complete");
        }

        public static async Task DownloadDependencies(Dictionary<string, string> dependencies)
        {
            ListRequest listRequest = Client.List();
            while (!listRequest.IsCompleted) await Task.Yield();
            IEnumerable<PackageInfo> packageInfos = listRequest.Result;

            foreach (var pair in dependencies)
            {
                if (!packageInfos.Any(p => p.name == pair.Key))
                {
                    string identifier = GetIdentifier(pair.Key, pair.Value);
                    await DownloadUnityPackage(identifier);
                }
            }
        }

        private static string GetIdentifier(string key, string value)
        {
            const string ID_FORMAT = "{0}@{1}";
            if (value.Contains("git")) return value.Replace("#path=", "?path=");
            else return string.Format(ID_FORMAT, key, value);
        }

        private static async Task DownloadUnityPackage(string identifier)
        {
            AddRequest request = Client.Add(identifier);
            while (!request.IsCompleted) await Task.Yield();

            if (request.Status != StatusCode.Success)
            {
                UnityEngine.Debug.Log($"Failed to install package {identifier}: {request.Error.message}");
            }
        }






        private static UnityWebRequest MakeRequest(string apiUrl)
        {
            UnityWebRequest request = UnityWebRequest.Get(apiUrl);

            string certiToken = KyowonCertificationManager.GetToken();
            string authHeader = "Bearer " + certiToken;
            request.SetRequestHeader("Authorization", authHeader);
            request.SetRequestHeader("Accept", "application/vnd.github+json");
            request.SetRequestHeader("X-GitHub-Api-Version", "2022-11-28");

            return request;
        }

        private static async Task<T> Request<T>(string apiUrl, IProgress<float> progress = null) where T : class
        {
            var request = MakeRequest(apiUrl);
            UnityWebRequestAsyncOperation task = request.SendWebRequest();
            while (!task.isDone)
            {
                progress?.Report(task.progress);
                await Task.Yield();
            }

            using (request)
            {
                if (string.IsNullOrEmpty(request.error))
                {
                    if (request.downloadHandler == null) return null;
                    if (typeof(T) == typeof(string)) return request.downloadHandler.text as T;            
                    return request.downloadHandler.data as T;
                }
                else
                {
                    UnityEngine.Debug.LogError("Error: " + request.error);
                    return null;
                }
            }
        }


        /// <summary>
        /// 압축파일이 있는 경로에 압축파일을 원하는 이름으로 풉니다. 푼 압축파일은 삭제합니다.
        /// </summary>
        /// <param name="tarballPath">압축파일 경로</param>
        /// <param name="rename">압축파일 풀었을 때 폴더이름</param>
        private async static Task UnpackTarball(string tarballPath, string rename)
        {
            await Task.Run(() =>
            {
                string targetDirectory = Directory.GetParent(tarballPath).FullName;
                ProcessStartInfo startInfo = new ProcessStartInfo("tar", $"-xzf {tarballPath} -C {targetDirectory}")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process { StartInfo = startInfo };
                process.Start();
                process.WaitForExit();

                string beforeFolder = Path.Combine(targetDirectory, "package");
                string afterFolder = Path.Combine(targetDirectory, rename);

                if (Directory.Exists(beforeFolder))
                {
                    Directory.Move(beforeFolder, afterFolder);
                    File.Delete(tarballPath);
                }
            });
        }
    }
}
