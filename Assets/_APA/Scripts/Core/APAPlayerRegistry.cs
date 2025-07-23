using System;
using System.Collections.Generic;
using UnityEngine;
namespace _APA.Scripts
{
    public class APAPlayerRegistry
    {
        public Transform Player1 { get; private set; }
        public Transform Player2 { get; private set; }

        private List<IPlayerReceiver> waitingReceivers = new();

        public void SetPlayers(Transform p1, Transform p2)
        {
            Player1 = p1;
            Player2 = p2;

            foreach (var receiver in waitingReceivers)
            {
                receiver.SetPlayers(p1, p2);
            }

            waitingReceivers.Clear();
        }

        public void RegisterReceiver(IPlayerReceiver receiver)
        {
            if (Player1 != null && Player2 != null)
            {
                receiver.SetPlayers(Player1, Player2);
            }
            else
            {
                waitingReceivers.Add(receiver);
            }
        }
    }
}


