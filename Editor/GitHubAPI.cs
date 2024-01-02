using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;

namespace KyowonPackageManager.Editor
{
    public class GitHubAPI
    {
        public static async Task<string> GetPacskageInfo(string packageName = null, string token = null)
        {
            const string PACKAGE_LIST_API_URL = "https://api.github.com/users/KyowonEduTech/packages?package_type=npm";
            const string PACKAGE_INFO_API_URL = "https://npm.pkg.github.com/@kyowonedutech/";

            string packageURL = (packageName == null) ? PACKAGE_LIST_API_URL : PACKAGE_INFO_API_URL;
            DownloadHandler handler = await SendRequest(MakeRequest(packageURL + packageName, token));

            return handler?.text;
        }

        public static async Task DownloadPackage(GitHubPackageDetailInfo packageDetailInfo)
        {
            EditorApplication.update += KyowonEditorWindow.UpdateProgressbar;

            string latestVesion = packageDetailInfo.dist_tags.Latest;
            //string latestVesion = packageDetailInfo.Versions.Values.ToList()[0].Version;
            string packageName = packageDetailInfo.Name;
            string url = packageDetailInfo.Versions[latestVesion].Dist.Tarball;

            UnityEngine.Debug.Log(packageName + "#############1");
            await Task.Delay(1000);
            DownloadHandler handler = await SendRequest(MakeRequest(url));
            UnityEngine.Debug.Log(packageName + "#############2");
            string packageFolderPath = KyowonPackageManager.GetModuleRootPath();
            UnityEngine.Debug.Log(packageName + "#############3");
            string downloadFile = Path.Combine(packageFolderPath, $"{packageName}.tar.gz");
            UnityEngine.Debug.Log(packageName + "#############4");
            byte[] tarballData = handler.data;
            UnityEngine.Debug.Log(packageName + "#############5");

            UnityEngine.Debug.Log(packageName + "#############");
            File.WriteAllBytes(downloadFile, tarballData);
            UnpackTarball(downloadFile, packageName);

            //if (hasUpdate)
            //{
            //    File.WriteAllBytes(downloadFile, tarballData);
            //    UnpackTarball(downloadFile, packageName);
            //}
            //if (!File.Exists(Path.Combine(packageFolderPath, packageName)))
            //{
            //    File.WriteAllBytes(downloadFile, tarballData);
            //    UnpackTarball(downloadFile, packageName);
                
            //}
            //else
            //{
            //    //폴더가 있고 version 이 낮으면 업데이트 해야 함.
            //    //지웠다가 깔아야 할 듯
            //}

            

            UnityEngine.Debug.Log($"{packageName} Download Complete");

            if (packageDetailInfo.Versions[latestVesion].Dependencies != null)
            {
                await DownloadDependencies(packageDetailInfo.Versions[latestVesion].Dependencies);
                UnityEngine.Debug.Log("All dependency packages installed!");
            }

            EditorApplication.update -= KyowonEditorWindow.UpdateProgressbar;
            EditorUtility.ClearProgressBar();

            Client.Resolve();
        }

        private static async Task DownloadDependencies(Dictionary<string, string> packageDictionary)
        {
            foreach (KeyValuePair<string, string> dependency in packageDictionary)
            {
                AddRequest request = Client.Add(dependency.Key);

                while (!request.IsCompleted)
                {
                    await Task.Delay(1000);
                }

                if (request.Status == StatusCode.Success)
                {
                    UnityEngine.Debug.Log($"Package {dependency.Key} installed successfully!");
                }
                else
                {
                    UnityEngine.Debug.Log($"Failed to install package {dependency.Key}: {request.Error.message}");
                    await DownloadDependencyWithValue(dependency.Value);
                }
            }
        }

        private static async Task DownloadDependencyWithValue(string packageValue)
        {
            packageValue = ChangeFormatToUrl(packageValue);
            AddRequest request2 = Client.Add(packageValue);

            while (!request2.IsCompleted)
            {
                await Task.Delay(1000);
            }

            if (request2.Status == StatusCode.Success)
            {
                UnityEngine.Debug.Log($"Package {packageValue} installed successfully!");
            }
            else
            {
                UnityEngine.Debug.LogError($"Failed to install package {packageValue}: {request2.Error.message}");
            }
        }

        private static string ChangeFormatToUrl(string text)
        {
            return text.Replace("#", "?");
        }

        /*
         * TODO: Github API Version 받아온 후 Info 요청 진행
        {
            string apiUrl = $"https://api.github.com/versions";
            StartCoroutine(SendRequest(MakeRequest(apiUrl)));
        }
        */

        private static UnityWebRequest MakeRequest(string apiUrl, string token = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(apiUrl);

            string certiToken = token ?? KyowonCertificationManager.GetToken();
            string authHeader = "Bearer " + certiToken;
            request.SetRequestHeader("Authorization", authHeader);
            request.SetRequestHeader("Accept", "application/vnd.github+json");
            request.SetRequestHeader("X-GitHub-Api-Version", "2022-11-28");

            return request;
        }

        private static async Task<DownloadHandler> SendRequest(UnityWebRequest request)
        {
            UnityWebRequestAsyncOperation task = request.SendWebRequest();
            while (!task.isDone)
            {
                await Task.Yield();
            }

            if (string.IsNullOrEmpty(request.error))
            {
                UnityEngine.Debug.Log(request.downloadHandler.text);
                return request.downloadHandler;
            }
            else
            {
                UnityEngine.Debug.LogError("Error: " + request.error);
                return null;
            }
        }

        /// <summary>
        /// 압축파일이 있는 경로에 압축파일을 원하는 이름으로 풉니다. 푼 압축파일은 삭제합니다.
        /// </summary>
        /// <param name="tarballPath">압축파일 경로</param>
        /// <param name="rename">압축파일 풀었을 때 폴더이름</param>
        private static void UnpackTarball(string tarballPath, string rename)
        {
            string targetDirectory = Directory.GetParent(tarballPath).FullName;
            ProcessStartInfo startInfo = new ProcessStartInfo("tar", $"-xzf {tarballPath} -C {targetDirectory}");
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

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
        }
    }
}