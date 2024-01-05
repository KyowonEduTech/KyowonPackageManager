using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace KyowonPackageManager.Editor
{
    enum WINDOW_TYPE
    {
        Certification,
        Download
    }

    public class KyowonEditorWindow : EditorWindow
    {
        private static WINDOW_TYPE _windowType = WINDOW_TYPE.Download;

        private const string EDITOR_CERTIFICATION_WINDOW_TITLE = "Kyowon Certification";
        private const string EDITOR_DOWNLOAD_WINDOW_TITLE = "Kyowon Package Manager";

        private const string GITHUB_TOKEN_GUIDE = "\nKyowon GitHub 인증키가 필요합니다.\n\n" +
                                          "1. Project 담당자에게 GitHub KyowonEduTech Repository 의 권한을 요청하세요.\n" +
                                          "2. GitHub 개인 계정에서 Personal Access Token(classic) 을 발급받아 입력하세요.\n" +
                                          "    발급 시, read:packages scope 를 추가해주어야 합니다.\n"+
                                          "3. 정상 인증 시, [Macintosh HD] - [사용자] - [유저이름] - .upmconfig.toml 파일이 생성됩니다.\n";
        private const string GITHUB_TOKEN_GUIDE_URL = "https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens";

        private const string GITHUB_PACKAGE_DOWNLOAD_GUIDE = "Kyowon Project Manager 를 다운로드하세요.";

        private const string KYOWON_PACKAGE_DOCUMENT_URL = "https://docs.google.com/document/d/1VA3VgsjUbBkwESblH3JFVOWyZQ6IfyG0ZwpgHqvPqcU/edit?usp=sharing";

        private static string _inputKey = "";
        private static List<GitHubPackageDetailInfo> _packageDetailList;


        public static KyowonEditorWindow Window { get; private set; }
        public static bool IsOpenedWindow
        {
            get { return Window != null; }
        }

        [MenuItem("Kyowon/Open Kyowon Package Manager")]
        private static async void ShowEditorWindow()
        {
            if (IsOpenedWindow) return;

            bool hasPermission = await KyowonCertificationManager.HasPackagePermission();
            if (!hasPermission) ShowCertificationWindow();
            else ShowDownloadWindow();
        }

        [MenuItem("Kyowon/Delete GitHub Certification File")]
        private static void DeleteCertiFile()
        {
            KyowonCertificationManager.DeleteCertiFile();
        }

        public static void ShowCertificationWindow()
        {
            Window = GetWindow<KyowonEditorWindow>();
            Window.titleContent = new GUIContent(EDITOR_CERTIFICATION_WINDOW_TITLE);
            _windowType = WINDOW_TYPE.Certification;
            Window.Show();
        }

        public static async void ShowDownloadWindow()
        {
            if (_packageDetailList == null)
            {
                EditorApplication.update += UpdateProgressbar;
                _packageDetailList = await KyowonPackageManager.GetPackageInfo();
                EditorApplication.update -= UpdateProgressbar;
                EditorUtility.ClearProgressBar();
            }
            Window = GetWindow<KyowonEditorWindow>();
            Window.titleContent = new GUIContent(EDITOR_DOWNLOAD_WINDOW_TITLE);
            _windowType = WINDOW_TYPE.Download;
            Window.Show();
        }

        private static float _progress = 0f;
        public static void UpdateProgressbar()
        {
            _progress += 0.01f;
            if (_progress > 1f) _progress = 1f;

            EditorUtility.DisplayProgressBar("Loading","", _progress);
        }

        private Vector2 _scrollPosition = Vector2.zero; //Editor Window 스크롤을 위한 변수
        private async void OnGUI()
        {
            Window.Focus();
            switch (_windowType)
            {
                case WINDOW_TYPE.Certification:
                    EditorGUILayout.HelpBox(GITHUB_TOKEN_GUIDE, MessageType.Warning);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUIStyle rightAlignedStyle = new GUIStyle(EditorStyles.miniButton);
                    rightAlignedStyle.alignment = TextAnchor.MiddleRight;

                    if (EditorGUILayout.LinkButton("GitHub Token 발급 가이드 Link"))
                    {
                        Application.OpenURL(GITHUB_TOKEN_GUIDE_URL);
                    }
                    GUILayout.EndHorizontal();

                    _inputKey = EditorGUILayout.TextField("Personal Access Token", _inputKey);
                    if (GUILayout.Button("Certification"))
                    {
                        bool hasPermission = await KyowonCertificationManager.HasPackagePermission(_inputKey);
                        if (hasPermission)
                        {
                            Close();
                            ShowEditorWindow();
                        }
                    }
                    break;
                case WINDOW_TYPE.Download:
                    if(_packageDetailList == null)
                    {
                        Close();
                        KyowonPackageManager.Start();
                    }

                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                    for (int i = 0; i < _packageDetailList?.Count; i++)
                    {
                        if (_packageDetailList[i].Name == "projectmanager")
                        {
                            DrawProjectManagerUI(_packageDetailList[i]);
                        }
                        else
                        {
                            if (!KyowonPackageManager.IsInstalled("projectmanager")) {
                                GUI.enabled = false;
                            }
                            DrawModuleInfo(_packageDetailList[i]);
                            GUI.enabled = true;
                        }
                        DrawLine();
                    }
                    EditorGUILayout.EndScrollView();
                    break;
            }
            DrawEditorWindowUpdateButton();
        }

        private void DrawProjectManagerUI(GitHubPackageDetailInfo package)
        {
            if (!KyowonPackageManager.IsInstalled("projectmanager"))
            {
                EditorGUILayout.HelpBox(GITHUB_PACKAGE_DOWNLOAD_GUIDE, MessageType.Info);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(GetLogoImage()); ;

            DrawModuleInfo(package);

            if (GUILayout.Button("Document"))
            {
                Application.OpenURL(KYOWON_PACKAGE_DOCUMENT_URL);
            }
            GUILayout.EndHorizontal();
        }

        private bool isFoldout = false; //Fold Button 변수
        private void DrawModuleInfo(GitHubPackageDetailInfo package)
        {
            EditorStyles.boldLabel.normal.textColor = Color.white;
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(package.Name, EditorStyles.boldLabel);

            #region Version Text
            string oldVersion = KyowonPackageManager.HasUpdate(package);
            if (oldVersion != null)
            {
                EditorGUILayout.LabelField(oldVersion);
            }
            else
            {
                EditorGUILayout.LabelField(package.dist_tags.Latest);
            }
            #endregion

            #region  추가설명 Fold Box
            isFoldout = EditorGUILayout.Foldout(isFoldout, "추가 설명");
            if (isFoldout)
            {
                EditorGUILayout.LabelField(package.Description);
            }
            #endregion

            GUILayout.EndVertical();

            #region Buttons
            if (!KyowonPackageManager.IsInstalled(package.Name))
            {
                DrawInstallButton(package);
            }
            else
            {
                if(oldVersion != null)
                {
                    DrawUpdateButton(package, package.dist_tags.Latest);
                }
                DrawRemoveButton(package);
            }
            #endregion

            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
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

        private void DrawLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        //TODO: 아래 함수 3개 하나로 정리
        private async void DrawInstallButton(GitHubPackageDetailInfo package)
        {
            if (GUILayout.Button("Install"))
            {
                await KyowonPackageManager.InstallPackage(package);
            }
        }

        private async void DrawUpdateButton(GitHubPackageDetailInfo package, string newVersion)
        {
            GUIStyle originalStyle = new GUIStyle(GUI.skin.button);
            GUIStyle coloredStyle = new GUIStyle(GUI.skin.button);
            coloredStyle.normal.textColor = Color.yellow;

            if (GUILayout.Button($"Update ({newVersion})", coloredStyle))
            {
                await KyowonPackageManager.InstallPackage(package);
            }

            GUI.skin.button = originalStyle;
        }

        private void DrawRemoveButton(GitHubPackageDetailInfo package)
        {
            if (GUILayout.Button("Remove"))
            {
                KyowonPackageManager.RemovePackage(package.Name);
            }
        }

        public void DrawEditorWindowUpdateButton()
        {
            if (GUILayout.Button(new GUIContent(" Editor Update", EditorGUIUtility.IconContent("Refresh@2x").image), EditorStyles.miniButton))
            {
                Close();
                KyowonPackageManager.RestartPackageManager();
            }
        }
    }
}

