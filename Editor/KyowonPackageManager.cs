using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine.Device;


namespace KyowonPackageManager.Editor
{
    [InitializeOnLoad]
    public static class KyowonPackageManager
    {
        private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "KyowonPackageManager");
        private static readonly string _fileName = "manifest.json";

        static KyowonPackageManager()
        {
            EditorApplication.update += Initialize;
        }

        public static void Initialize()
        {
            if (KyowonEditorWindow.IsOpenedWindow || IsInstalled("com.kyowon.unityplugins.projectmanager"))
            {
                return;
            }

            if (!KyowonCertificationManager.HasPackagePermission())
            {
                KyowonEditorWindow.ShowCertificationWindow();
            }
            else
            { 
                KyowonEditorWindow.ShowDownloadWindow();
            }
        }

        public static async Task<List<GitHubPackageInfo>> GetPackageInfo()
        {
            string packageInfoText = await GitHubAPI.GetPacskageInfo();
            return JsonConvert.DeserializeObject<List<GitHubPackageInfo>>(packageInfoText); 
        }

        public static async Task<GitHubPackageDetailInfo> GetPackageDetailInfo(string packageName)
        {
            string packageDetailInfoText = await GitHubAPI.GetPacskageInfo(packageName);
            return JsonConvert.DeserializeObject<GitHubPackageDetailInfo>(packageDetailInfoText);
        }

        public static async Task InstallPackage(string packageName)
        {
            GitHubPackageDetailInfo packageDetailInfo = await GetPackageDetailInfo(packageName);
            await GitHubAPI.DownloadPackage(packageDetailInfo);
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
            string packageFolderPath = Path.Combine(Application.dataPath, "KyowonModules");
            string searchFilePath = Path.Combine(packageFolderPath, packageName);

            if (Directory.Exists(searchFilePath))
            {
                return true;
            }
            return false;
        }

        //download 된 pakcage 관리 파일 생성
        private static void SetManifest(string packageName)
        {
            if (File.Exists(Path.Combine(_path, _fileName))){

            }
            else
            {
                File.WriteAllText(Path.Combine(_path, _fileName), null);
            }
        }

       
    }
}