namespace QArantineLauncher.Code.Projects.Running
{
    public class ProcessInfo(int pID, string name)
    {
        public int PID { get; set; } = pID;
        public string Name { get; set; } = name;

        public override string ToString()
        {
            return $"{PID} - {Name}";
        }
    }
}