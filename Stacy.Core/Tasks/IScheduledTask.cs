namespace Stacy.Core.Tasks
{
    public interface IScheduledTask
    {
        void RunTask(int accomid = -1);
    }
}
