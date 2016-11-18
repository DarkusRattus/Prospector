using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PlayerType
{
    human,
    ai
}

// The individual Player of the game
// Note: The Player does NOT extend MonoBehavior (or any other class)

public class Player {

    public PlayerType type = PlayerType.ai;
    public int playerNum;

    public List<CardBartok> hand; // The cards in this player's hand

    public BSlotDef handBSlotDef;
    
    // Add a Card to the hand
    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand == null) hand = new List<CardBartok>();

        // Add the card to the hand
        hand.Add(eCB);
        return (eCB);
    }

    // Remove a Card from the hand
    public CardBartok RemoveCard(CardBartok cb)
    {
        hand.Remove(cb);
        return (cb);
    }
}
