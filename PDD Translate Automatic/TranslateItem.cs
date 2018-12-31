namespace PDD_Translate_Automatic
{
    class TranslateItem
    {
        public object Locker = new object();
        public bool Done = false;
        public string Result;

        public TranslateItem() { }

        public TranslateItem(string result)
        {
            Finish(result);
        }

        public void Finish(string result)
        {
            Done = true;
            Result = result;
        }
    }
}
