using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;



namespace Kyowon.Package.UI
{
    public class CertificationWindow : EditorWindow
    {

        private const string CERTIFICATION_WINDOW_TITLE = "Kyowon Certification Window";
        private const string GITHUB_TITLE_TEXT = "Kyowon GitHub 인증키가 필요합니다.";
        private const string GITHUB_TOKEN_GUIDE = "1. Project 담당자에게 GitHub KyowonEduTech Repository 의 권한을 요청하세요.\n" +
                                                  "2. GitHub 개인 계정에서 Personal Access Token(classic) 을 발급받아 입력하세요.\n" +
                                                  "    발급 시, read:packages scope 를 추가해주어야 합니다.\n" +
                                                  "3. 정상 인증 시, [Macintosh HD] - [사용자] - [유저이름] - .upmconfig.toml 파일이 생성됩니다.\n";
        private const string CERTI_BUTTON_TEXT = "Certification";
        private const string GITHUB_TOKEN_GUIDE_URL = "https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens";
        private const string DOCUMNE_BUTTON_TEXT = "GitHub Token 발급 가이드 Link";
        private static string _inputKey = "";

        [SerializeField] VisualTreeAsset _asset;

        private Label TitleLabel { get { return rootVisualElement.Q<Label>("Title"); } }
        private Label DescriptLabel { get { return rootVisualElement.Q<Label>("Description"); } }
        private TextField TokenInputFeild { get { return rootVisualElement.Q<TextField>("TokenInputFeild"); } }
        private Button CertiButton { get { return rootVisualElement.Q<Button>("CertiButton"); } }
        private Button DocumentURLButton { get { return rootVisualElement.Q<Button>("DocumentURLButton"); } }



        public static void ShowWindow()
        {
            CertificationWindow window = GetWindow<CertificationWindow>();
            window.titleContent = new GUIContent(CERTIFICATION_WINDOW_TITLE);
            window.minSize = new Vector2(600, 200);
            window.maxSize = new Vector2(600, 200);
        }

        public void CreateGUI()
        {
            _asset.CloneTree(rootVisualElement);

            //Text
            TitleLabel.text = GITHUB_TITLE_TEXT;
            DescriptLabel.text = GITHUB_TOKEN_GUIDE;
            TokenInputFeild.value = "";
            CertiButton.text = CERTI_BUTTON_TEXT;
            DocumentURLButton.text = DOCUMNE_BUTTON_TEXT;

            //Event
            CertiButton.clickable.clicked += Certificate;
            DocumentURLButton.clickable.clicked += OpenURL;
        }

        private void Certificate() { CertificateAsync(); }
        private async void CertificateAsync()
        {
            _inputKey = TokenInputFeild.text;

            bool hasPermission = await KyowonCertificationManager.HasPackagePermission(_inputKey);
            if (hasPermission)
            {
                Close();
                KyowonPackageManager.ShowEditorWindow();
            }
        }

        private void OpenURL()
        {
            Application.OpenURL(GITHUB_TOKEN_GUIDE_URL);
        }

        [MenuItem("Kyowon/Package/Delete GitHub Certification File")]
        private static void DeleteCertiFile()
        {
            KyowonCertificationManager.DeleteCertiFile();
        }
    }
}