namespace TP.ConcurrentProgramming.Data
{
    public class BallMoveEvent
    {
        public IVector position;
        public double deltatime;

        public BallMoveEvent(IVector position, double deltatime) {
            this.position = position;
            this.deltatime = deltatime;
        }
    }
}
