namespace PyramidSolitaireSagaSample.Helper
{
    public class EnumState<T>
    {
        public bool IsStateChanged;
        public T CurrState
        {
            get;
            private set;
        }
        public T PrevState
        {
            get;
            private set;
        }
        public string CurrStateStr
        {
            get;
            private set;
        }
        public string PrevStateStr
        {
            get;
            private set;
        }

        private bool showLog;
        public EnumState(bool showLog = false)
        {
            this.showLog = showLog;
        }

        public void Set(T nextState)
        {
            PrevState = CurrState;
            CurrState = nextState;

            PrevStateStr = PrevState.ToString();
            CurrStateStr = CurrState.ToString();

            IsStateChanged = PrevStateStr != CurrStateStr;
            if (showLog
                && IsStateChanged)
            {
                //Debug.Log($"Set : {PrevStateStr} -> {CurrStateStr} : {IsStateChanged}");
            }
        }
    }
}