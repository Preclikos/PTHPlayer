namespace PTHPlayer.Controllers.Enums
{
    public enum SubscriptionStatus
    {
        //New state can be cancelled every time
        New,
        //Submitted sub is send and accepted but player not started need unsub but not player cancell
        Submitted,
        //Submitted and calculation is done cannot be cancelled need wait for play
        WaitForPlay,
        //Player run need stop player then unsub and clean storage
        Play,
        Pause
    }
}
