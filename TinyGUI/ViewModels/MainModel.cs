namespace TinyGUI.ViewModels
{
    public class MainModel : ViewModelBase
    {
        private bool _compressRadioButtonIsChecked = true;

        public bool CompressRadioButtonIsChecked
        {
            get => _compressRadioButtonIsChecked;
            set => SetField(ref _compressRadioButtonIsChecked, value, nameof(CompressRadioButtonIsChecked));
        }

        private bool _smartCutRadioButtonIsChecked;

        public bool SmartCutRadioButtonIsChecked
        {
            get => _smartCutRadioButtonIsChecked;
            set => SetField(ref _smartCutRadioButtonIsChecked, value, nameof(SmartCutRadioButtonIsChecked));
        }

        private bool _settingRadioButtonIsChecked;

        public bool SettingRadioButtonIsChecked
        {
            get => _settingRadioButtonIsChecked;
            set => SetField(ref _settingRadioButtonIsChecked, value, nameof(SettingRadioButtonIsChecked));
        }


        #region 智能剪切

        private string _imageWidth = string.Empty;

        public string ImageWidth
        {
            get => _imageWidth;
            set => SetField(ref _imageWidth, value, nameof(ImageWidth));
        }

        private string _imageHeight = string.Empty;

        public string ImageHeight
        {
            get => _imageHeight;
            set => SetField(ref _imageHeight, value, nameof(ImageHeight));
        }

        private bool _scaleRadioButtonIsChecked = true;

        public bool ScaleRadioButtonIsChecked
        {
            get => _scaleRadioButtonIsChecked;
            set => SetField(ref _scaleRadioButtonIsChecked, value, nameof(ScaleRadioButtonIsChecked));
        }

        private bool _fitRadioButtonIsChecked;

        public bool FitRadioButtonIsChecked
        {
            get => _fitRadioButtonIsChecked;
            set => SetField(ref _fitRadioButtonIsChecked, value, nameof(FitRadioButtonIsChecked));
        }

        private bool _coverRadioButtonIsChecked;

        public bool CoverRadioButtonIsChecked
        {
            get => _coverRadioButtonIsChecked;
            set => SetField(ref _coverRadioButtonIsChecked, value, nameof(CoverRadioButtonIsChecked));
        }

        private bool _thumbRadioButtonIsChecked;

        public bool ThumbRadioButtonIsChecked
        {
            get => _thumbRadioButtonIsChecked;
            set => SetField(ref _thumbRadioButtonIsChecked, value, nameof(ThumbRadioButtonIsChecked));
        }

        #endregion

        private bool _isIndeterminate = false;

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => SetField(ref _isIndeterminate, value, nameof(IsIndeterminate));
        }

        private double _progressBarValue = 0.0;

        public double ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                if (value > 0)
                {
                    IsIndeterminate = false;
                }

                SetField(ref _progressBarValue, value, nameof(ProgressBarValue));
            }
        }
    }
}