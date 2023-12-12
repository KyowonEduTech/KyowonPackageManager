using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace KyowonPackageManager.Editor
{
    public static class KyowonPackageManager
    {
        private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "KyowonPackageManager");
        private const string MANIFEST_FILE_NAME = "manifest.json";

        private static List<GitHubPackageDetailInfo> _packageDetailInfoList = new List<GitHubPackageDetailInfo>();

        public static async void Start()
        {
            if (KyowonEditorWindow.IsOpenedWindow) return;

            bool hasPermission = await KyowonCertificationManager.HasPackagePermission();
            if (!hasPermission) KyowonEditorWindow.ShowCertificationWindow();
            else KyowonEditorWindow.ShowDownloadWindow();
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
                    GitHubPackageDetailInfo packageDetailInfo = JsonConvert.DeserializeObject<GitHubPackageDetailInfo>(packageDetailInfoText);
                    packageDetailInfo.Name = Rename(packageDetailInfo.Name);
                    _packageDetailInfoList.Add(packageDetailInfo);
                }
            }
            return _packageDetailInfoList;
        }

        private static string Rename(string fullName)
        {
            string removeText = "com.kyowon.unityplugins.";
            return fullName.Remove(removeText.IndexOf(removeText), removeText.Length);
        }

        public static async Task<GitHubPackageDetailInfo> GetPackageDetailInfo(string packageName)
        {
            string packageDetailInfoText = await GitHubAPI.GetPacskageInfo(packageName);
            return JsonConvert.DeserializeObject<GitHubPackageDetailInfo>(packageDetailInfoText);
        }

        public static async Task InstallPackage(GitHubPackageDetailInfo package)
        {
            await GitHubAPI.DownloadPackage(package);
        }

        public static void RemovePackage()
        {
            //패키지 있는지 확인
            //다운로드 관리 파일에 패키지이름/버전 추가
            //파일 지우기
        }

        public static void UpdatePackage()
        {
            //파일 덮어쓰기
        }

        public static bool IsInstalled(string packageName)
        {
            string packageFolderPath = Path.Combine(UnityEngine.Application.dataPath, "KyowonModules");
            string searchFilePath = Path.Combine(packageFolderPath, packageName);

            return Directory.Exists(searchFilePath) ? true : false;
        }
        
        //download 된 pakcage 관리 파일 생성
        private static void SetManifest(string packageName)
        {
            if (File.Exists(Path.Combine(_path, MANIFEST_FILE_NAME)))
            {

            }
            else
            {
                File.WriteAllText(Path.Combine(_path, MANIFEST_FILE_NAME), null);
            }
        }
    }
}