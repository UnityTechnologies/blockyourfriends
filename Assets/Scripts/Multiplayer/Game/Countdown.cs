using System;
using UnityEngine;

namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Runs the countdown to the in-game state. While the start of the countdown is synced via Relay, the countdown itself is handled locally,
    /// since precise timing isn't necessary.
    /// </summary>
    [RequireComponent(typeof(UI.CountdownUI))]
    public class Countdown : MonoBehaviour, IReceiveMessages
    {
        public class Data : Observed<Countdown.Data>
        {
            private float timeLeft;
            public float TimeLeft 
            {
                get => timeLeft;
                set
                {   timeLeft = value;
                    OnChanged(this);
                }
            }
            public override void CopyObserved(Data oldObserved) { /*No-op, since this is unnecessary.*/ }
        }

        private Data data = new Data();
        private UI.CountdownUI ui;
        private const int k_countdownTime = 4;

        public void OnEnable()
        {
            if (ui == null)
                ui = GetComponent<UI.CountdownUI>();

            data.TimeLeft = -1;
            Locator.Get.Messenger.Subscribe(this);
            ui.BeginObserving(data);
        }

        public void OnDisable()
        {
            Locator.Get.Messenger.Unsubscribe(this);
            ui.EndObserving();
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.StartCountdown)
            {
                data.TimeLeft = k_countdownTime;
            }
            else if (type == MessageType.CancelCountdown)
            {
                data.TimeLeft = -1;
            }
        }

        public void Update()
        {
            if (data.TimeLeft < 0)
                return;

            data.TimeLeft -= Time.deltaTime;

            if (data.TimeLeft < 0)
                Locator.Get.Messenger.OnReceiveMessage(MessageType.CompleteCountdown, null);
        }
    }
}
