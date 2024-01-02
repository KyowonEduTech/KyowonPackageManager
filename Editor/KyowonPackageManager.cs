using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;


namespace KyowonPackageManager.Editor
{
    public static class KyowonPackageManager
    {
        private static readonly string _moduleRootPath = Path.Combine(Application.dataPath, "../Packages");
        private static List<GitHubPackageDetailInfo> _packageDetailInfoList = new List<GitHubPackageDetailInfo>();
        private static KyowonPackage _kyowonPackage;

        public static async void Start()
        {
            if (KyowonEditorWindow.IsOpenedWindow) return;

            bool hasPermission = await KyowonCertificationManager.HasPackagePermission();
            if (!hasPermission) KyowonEditorWindow.ShowCertificationWindow();
            KyowonEditorWindow.ShowDownloadWindow();
        }

        public static async Task<List<GitHubPackageDetailInfo>> GetPackageInfo()
        {
            if (_packageDetailInfoList.Count == 0)
            {
                string packageInfoText = await GitHubAPI.GetPacskageInfo();
                List<GitHubPackageInfo> _packageList = JsonConvert.DeserializeObject<List<GitHubPackageInfo>>(packageInfoText);

                foreach (GitHubPackageInfo packageInfo in _packageList)
                {
                    string packageDetailInfoText = await GitHubAPI.GetPacskageInfo(packageInfo.Name);
                    string removeText = "com.kyowon.unityplugins.";

                    GitHubPackageDetailInfo packageDetailInfo = JsonConvert.DeserializeObject<GitHubPackageDetailInfo>(packageDetailInfoText);

                    packageDetailInfo.Name = packageDetailInfo.Name.Remove(removeText.IndexOf(removeText), removeText.Length);
                    _packageDetailInfoList.Add(packageDetailInfo);
                }
            }
            return _packageDetailInfoList;
        }

        public static async Task<GitHubPackageDetailInfo> GetPackageDetailInfo(string packageName)
        {
            string packageDetailInfoText = await GitHubAPI.GetPacskageInfo(packageName);
            return JsonConvert.DeserializeObject<GitHubPackageDetailInfo>(packageDetailInfoText);
        }

        public static string GetModuleRootPath()
        {
            return _moduleRootPath;
        }

        public static async Task InstallPackage(GitHubPackageDetailInfo package)
        {
            string modulePath = Path.Combine(_moduleRootPath, package.Name);

            if (Directory.Exists(modulePath))
            {
                RemovePackage(package.Name);
            }
            bool hasUpdate = HasUpdateKyowonPackageInfo(package);
            if (hasUpdate)
            {
                await GitHubAPI.DownloadPackage(package);
            }
        }

        public static void RemovePackage(string packageName)
        { 
            string modulePath = Path.Combine(_moduleRootPath, packageName);
 
            if (Directory.Exists(modulePath)) { 
                Directory.Delete(modulePath, true);
                File.Delete(modulePath + ".meta");
            }
            RemoveKyowonPackageInfo(packageName);

            AssetDatabase.Refresh();
        }

        public static string HasUpdate(GitHubPackageDetailInfo package)
        {
            string packagePath = Path.Combine(_moduleRootPath, package.Name);
            if (Directory.Exists(packagePath))
            {
                string jsonText = File.ReadAllText(Path.Combine(packagePath, "package.json"));
                _kyowonPackage = JsonConvert.DeserializeObject<KyowonPackage>(jsonText);
                int result = string.Compare(package.dist_tags.Latest, _kyowonPackage.Version);

                if(result > 0)
                {
                    return _kyowonPackage.Version;
                }
            }
            return null;
        }


        private static bool HasUpdateKyowonPackageInfo(GitHubPackageDetailInfo packageDetailInfo)
        {
            string infoPath = Path.Combine(_moduleRootPath, "KyowonPackageInfo.json");

            List<GitHubPackageDetailInfo> infos = new List<GitHubPackageDetailInfo>();
            if (File.Exists(infoPath))
            {
                string text = File.ReadAllText(infoPath);
                infos = JsonConvert.DeserializeObject<List<GitHubPackageDetailInfo>>(text);
            }

            if (infos.Find((x) => x.Name == packageDetailInfo.Name) is GitHubPackageDetailInfo find)
            {
                if (find.dist_tags.Latest != packageDetailInfo.dist_tags.Latest)
                {
                    infos.Remove(find);
                    infos.Add(packageDetailInfo);
                    string json = JsonConvert.SerializeObject(infos);
                    File.WriteAllText(infoPath, json);
                    AssetDatabase.Refresh();

                    return true;
                }
            }
            else
            {
                infos.Add(packageDetailInfo);
                string json = JsonConvert.SerializeObject(infos);
                File.WriteAllText(infoPath, json);
                AssetDatabase.Refresh();
                return true;
            }

            AssetDatabase.Refresh();
            return false;
        }

        private static void RemoveKyowonPackageInfo(string packageName)
        {
            string infoPath = Path.Combine(_moduleRootPath, "KyowonPackageInfo.json");
            if (File.Exists(infoPath))
            {
                string text = File.ReadAllText(infoPath);
                List<GitHubPackageDetailInfo> infos = JsonConvert.DeserializeObject<List<GitHubPackageDetailInfo>>(text);
                if (infos.Find((x) => x.Name == packageName) is GitHubPackageDetailInfo find)
                {
                    infos.Remove(find);
                    string json = JsonConvert.SerializeObject(infos);
                    File.WriteAllText(infoPath, json);
                    AssetDatabase.Refresh();
                    Debug.Log("Delete!!!!!!!!!!!!!!!!!!!!!");
                }
            }
        }

        public static bool IsInstalled(string packageName)
        {
            string module = Path.Combine(_moduleRootPath, packageName);
            return Directory.Exists(module);
        }

        public static void RestartPackageManager()
        {
            _packageDetailInfoList = null;
            Start();
        }
    }
}