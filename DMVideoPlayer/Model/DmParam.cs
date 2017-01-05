namespace DMVideoPlayer.Model
{
    public class DmParam
    {
        public DmParam(string v1, string v2)
        {
            this.name = v1;
            this.value = v2;
        }

        public string name { get; set; }
        public string value { get; set; }
    }
}