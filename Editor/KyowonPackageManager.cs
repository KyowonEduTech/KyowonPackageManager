using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


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
            await GitHubAPI.DownloadPackage(package);
        }

        public static void RemovePackage(string packageName)
        { 
            string modulePath = Path.Combine(_moduleRootPath, packageName);
 
            if (Directory.Exists(modulePath)) { 
                Directory.Delete(modulePath, true);
                File.Delete(modulePath + ".meta");
            }
            AssetDatabase.Refresh();
        }

        public static bool HasUpdate(GitHubPackageDetailInfo package)
        {
            string packagePath = Path.Combine(_moduleRootPath, package.Name);
            if (Directory.Exists(packagePath))
            {
                string jsonText = File.ReadAllText(Path.Combine(packagePath, "package.json"));
                _kyowonPackage = JsonConvert.DeserializeObject<KyowonPackage>(jsonText);
                int result = string.Compare(package.dist_tags.Latest, _kyowonPackage.Version);

                if(result > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsInstalled(string packageName)
        {
            string module = Path.Combine(_moduleRootPath, packageName);
            return Directory.Exists(module);
        }
    }
}