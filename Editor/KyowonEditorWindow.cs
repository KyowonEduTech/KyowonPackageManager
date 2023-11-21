using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace KyowonPackageManager.Editor
{
    public class KyowonEditorWindow : EditorWindow
    {
        private const string EDITOR_CERTIFICATION_WINDOW_TITLE = "Kyowon Certification Windnow";
        private const string EDITOR_DOWNLOAD_WINDOW_TITLE = "Kyowon Package Download Windnow";

        private const string GITHUB_TOKEN_GUIDE = "Kyowon GitHub 인증키가 필요합니다.\n\n" +
                                          "1. Project 담당자에게 GitHub KyowonEduTech Repository 의 권한을 요청하세요.\n" +
                                          "2. GitHub 개인 계정에서 Personal Access Token 을 발급받아 입력하세요.\n";
        private const string GITHUB_PACKAGE_DOWNLOAD_GUIDE = "Kyowon Project Manager 를 다운로드하세요.";

        private const string KYOWON_PACKAGE_DOCUMENT_URL = "https://docs.google.com/document/d/1VA3VgsjUbBkwESblH3JFVOWyZQ6IfyG0ZwpgHqvPqcU/edit?usp=sharing";

        public static KyowonEditorWindow Window { get; private set; }
        public static bool IsOpenedWindow
        {
            get { return Window != null; }
        }

        [MenuItem("Kyowon/Kyowon Package Manager")]
        private static void ShowEditorWindow()
        {
            if (IsOpenedWindow)
            {
                return;
            }

            if (!KyowonCertificationManager.HasPackagePermission())
            {
                ShowCertificationWindow();
            }
            else
            {
                ShowDownloadWindow();
            }
        }

        public static void ShowCertificationWindow()
        {
            Window = GetWindow<KyowonEditorWindow>();
            Window.titleContent = new GUIContent(EDITOR_CERTIFICATION_WINDOW_TITLE);
            Window.Show();
        }

        public static void ShowDownloadWindow()
        {
            Window = GetWindow<KyowonEditorWindow>();
            Window.titleContent = new GUIContent(EDITOR_DOWNLOAD_WINDOW_TITLE);
            Window.Show();
        }

        private string _inputKey = "";
        private List<GitHubPackageInfo> _packageList;

        private void OnGUI()
        {
            if (!KyowonCertificationManager.HasPackagePermission())
            {
                Window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 115);
                EditorGUILayout.HelpBox(GITHUB_TOKEN_GUIDE, MessageType.Warning);

                _inputKey = EditorGUILayout.TextField("Personal Access Token", _inputKey);
                if (GUILayout.Button("Certification"))
                {
                    Close();
                    KyowonCertificationManager.HasPackagePermission(_inputKey);
                }
            }
            else
            {
                Window.position = new Rect(Screen.width / 2, Screen.height / 2, 600, 130);

                GetPackageInfo();
                if (_packageList != null)
                {
                    EditorGUILayout.HelpBox(GITHUB_PACKAGE_DOWNLOAD_GUIDE, MessageType.Info);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(GetLogoImage());

                    //TODO: com.kyowon.unityplugins.projectmanager 인지 체크
                    EditorGUILayout.LabelField(_packageList[0].Name);
                    if (GUILayout.Button("Install"))
                    {
                        InstallPackage(_packageList[0].Name);
                    }
                    if (GUILayout.Button("Document"))
                    {
                        Application.OpenURL(KYOWON_PACKAGE_DOCUMENT_URL);
                    }
                    EditorGUILayout.EndHorizontal();


                    //Set UI - Kyowon Modules
                    //MakePacakgeObject();
                }
            }
        }

        private async void GetPackageInfo()
        {
            _packageList = await KyowonPackageManager.GetPackageInfo();
        }

        private async void InstallPackage(string packageName)
        {
            await KyowonPackageManager.InstallPackage(packageName);
        }

        private Texture2D GetLogoImage()
        {
            MonoScript script = MonoScript.FromScriptableObject(this);
            string scriptFilePath = AssetDatabase.GetAssetPath(script);

            FileInfo fileInfo = new FileInfo(scriptFilePath);
            string logoPath = fileInfo.Directory.ToString();

            var imageBytes = File.ReadAllBytes(Path.Combine(logoPath, "logo.jpg"));
            Texture2D texture = new Texture2D(100, 100);
            texture.LoadImage(imageBytes);

            return texture;
        }

    }
}