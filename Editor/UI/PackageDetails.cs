using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kyowon.Package.UI
{
    public class PackageDetails : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<PackageDetails> { }

        private const string UPDATE_FORMAT = "Update ({0})";



        private Label TitleLabel { get { return this.Q<Label>("detailTitle"); } }
        private Button InstallButton { get { return this.Q<Button>("packageInstallButton"); } }
        private Button RemoveButton { get { return this.Q<Button>("packageRemoveButton"); } }
        private Button UpdateButton { get { return this.Q<Button>("packageUpdateButton"); } }
        private Button DocumentButton { get { return this.Q<Button>("DocumentButton"); } }

        private Label VersionLabel { get { return this.Q<Label>("detailVersion"); } }
        private Label PackageLabel { get { return this.Q<Label>("detailName"); } }
        private Label DetailDesc { get { return this.Q<Label>("detailDescription"); } }

        private const string KYOWON_PACKAGE_DOCUMENT_URL = "https://docs.google.com/document/d/1VA3VgsjUbBkwESblH3JFVOWyZQ6IfyG0ZwpgHqvPqcU/edit?usp=sharing";

        public KyowonPackageInfo Info { get; private set; }
        public event Action<KyowonPackageInfo> OnUpdated = delegate { };


        public PackageDetails()
        {
            var asset = Resources.Load<VisualTreeAsset>("PackageDetails");
            asset.CloneTree(this);

            UIUtils.SetElementDisplay(this, false);

            UIUtils.SetElementDisplay(InstallButton, false);
            UIUtils.SetElementDisplay(RemoveButton, false);
            UIUtils.SetElementDisplay(UpdateButton, false);
            UIUtils.SetElementDisplay(DocumentButton, true);

            InstallButton.clickable.clicked += InstallPackage;
            RemoveButton.clickable.clicked += RemovePackage;
            UpdateButton.clickable.clicked += InstallPackage;
            DocumentButton.clickable.clicked += ShowDocument;
        }

        public void SetPackage(KyowonPackageInfo info, bool force = false)
        {
            if (info == null) return;
            if (!force && Info == info) return;
            Info = info;
            UIUtils.SetElementDisplay(this, true);

            TitleLabel.text = info.DisplayName ?? info.Name;

            UIUtils.SetElementDisplay(InstallButton, !info.IsInstalled);
            UIUtils.SetElementDisplay(RemoveButton, info.IsInstalled);
            UIUtils.SetElementDisplay(UpdateButton, info.HasUpdate);
            UIUtils.SetElementDisplay(DocumentButton, true);

            if (info.HasUpdate)
            {
                UpdateButton.text = string.Format(UPDATE_FORMAT);
            }

            VersionLabel.text = info.Version ?? info.Latest;
            PackageLabel.text = info.Name;

            DetailDesc.text = info.Description;
        }

        private void InstallPackage() { InstallPackageAsync(); }

        private void RemovePackage()
        {
            KyowonPackageManager.RemovePackage(Info);
            SetPackage(Info, true);
            OnUpdated(Info);
        }


        private async void InstallPackageAsync()
        {
            await KyowonPackageManager.InstallPackage(Info, Info.Latest);
            SetPackage(Info, true);
            OnUpdated(Info);
        }

        private void ShowDocument()
        {
            Application.OpenURL(KYOWON_PACKAGE_DOCUMENT_URL);
        }
    }
}