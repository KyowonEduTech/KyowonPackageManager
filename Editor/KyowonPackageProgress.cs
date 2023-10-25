using System;
using UnityEditor;

namespace Kyowon.Package
{
    public class KyowonPackageProgress : IProgress<float>
    {
        public const string TITLE = "Kyowon Package Manager";
        public const string MSG_LOAD_LIST = "Get package list from Github";
        public const string MSG_SYNC_PACKAGES = "Sync kyowon packages";
        public const string MSG_INSTALL_PACKAGE = "Install package - {0}";


        private KyowonPackageProgress _parent;
        private string _message;
        private float _min = 0f, _max = 1f;


        public KyowonPackageProgress(string message)
        {
            _message = message;
            Report(_min);
        }

        public KyowonPackageProgress CreateSubProgress(string message, float min, float max)
        {
            message ??= _message;
            return new KyowonPackageProgress(message)
            {
                _parent = this,
                _min = min,
                _max = max
            };
        }


        public void Report(float value)
        {
            value *= _max - _min;
            value += _min;

            if (_parent != null) _parent.Report(value);
            else
            {
                if (value >= 0) EditorUtility.DisplayProgressBar(TITLE, _message, value);
                if (value >= 1) EditorUtility.ClearProgressBar();
            }
        }

        public void SetComplete()
        {
            Report(_max);
        }
    }
}
