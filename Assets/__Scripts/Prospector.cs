﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector S;

	public Deck	deck;
	public TextAsset deckXML;

    public Layout layout;
    public TextAsset layoutXML;
    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;

    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;  

	void Awake(){
		S = this;
	}

    public List<CardProspector> drawPile;

	void Start() {
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle (ref deck.cards);

        layout = GetComponent<Layout>(); // Get the Layout
        layout.ReadLayout(layoutXML.text); // Pass LayoutXML to it

        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
	}

    CardProspector Draw()
    {
        CardProspector cd = drawPile[0]; // Pull the 0th CardProspector
        drawPile.RemoveAt(0);            // Then remove it from the List<> drawPile
        return (cd);                     // And return it
    }

    // Convert from the layoutID int to the CardProspector with that ID
    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach(CardProspector tCP in tableau)
        {
            // Search through all cards in the tableau List<>
            if(tCP.layoutID == layoutID)
            {
                // If the card has the same ID, return it
                return tCP;
            }
            // If it's not found, return null
            return (null);
        }
    }

    // LayoutGame() positions the initial tableau of cards, a.k.a. the "mine"
    void LayoutGame()
    {
        // Create an empty GameObject to serve as an anchor for the tableau
        if(layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            // ^ Creates an empty GameObject named _LayoutAnchor in the Hierarchy
            layoutAnchor = tGO.transform; // Grab its transformation
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        // Follow the layout
        foreach (SlotDef tSD in layout.slotDefs)
        {
            // ^ Iterate through all the SlotDefs in the layout.slotDefs as tSD
            cp = Draw(); // Pull a card from the top (beginning) of the drawPile
            cp.faceUp = tSD.faceUp; // Set its faceUp to the value in SlotDef
            cp.transform.parent = layoutAnchor; // Make its parent layoutAnchor
            // This replaces the previous parent: deck.deckAnchor, which appears as _Deck in the Hierarchy while the scene is playing
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);
            // ^ Set the localPosition of the card based on slotDef
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = CardState.tableau;
            // CardProspectors in the tableau have the state CardState.tableau

            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp); // Add this CardProspector to the List<> tableau
        }

        // Set which cards are hiding others
        foreach(CardProspector tCP in tableau)
        {
            foreach(int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        // Set up the initial target card
        MoveToTarget(Draw());

        // Set up the Draw pile
        UpdateDrawPile();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach(Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return (lCP);
    }

    // CardClicked is called any time a card in the game is clicked
    public void CardClicked(CardProspector cd)
    {
        // The reaction is determined by the state of the clicked card
        switch (cd.state)
        {
            case CardState.target:
                // Clicking on the target card does nothing
                break;
            case CardState.drawpile:
                // Clicking any card in the drawPile will draw the next card
                MoveToDiscard(target); // Moves the target to the discardPile
                MoveToTarget(Draw());  // Moves the next drawn card to the target
                UpdateDrawPile();      // Restacks the drawPile
                break;
            case CardState.tableau:
                // Clicking a card in the tableau will check if it's a valid play
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    //If card is face-down, it's not valid
                    validMatch = false;
                }
                if(!AdjacentRank(cd, target))
                {
                    // If it's not an adjacent rank, it's not valid
                    validMatch = false;
                }
                if (!validMatch) return; // return if not valid
                // Yay! It's a valid card.
                tableau.Remove(cd); // Remove it from the tableau List
                MoveToTarget(cd); // Make it the target card
                SetTableauFaces(); // Update tableau card face-ups
                break;
        }
    }

    // Moves the current target to the discardPile
    void MoveToDiscard(CardProspector cd)
    {
        // Set the state of the card to discard
        cd.state = CardState.discard;
        discardPile.Add(cd); // Add it to the discardPile List<>
        cd.transform.parent = layoutAnchor; // Update its transform parent
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        // ^ Position it on top of the pile for depth sorting
        cd.faceUp = true;
        // Place it on top of the pile for depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    // Make c the new target card
    void MoveToTarget(CardProspector cd)
    {
        // If there is currently a target card, move it to discardPile
        if (target != null) MoveToDiscard(target);
        target = cd; // cd is the new target
        cd.state = CardState.target;
        cd.transform.parent = layoutAnchor;
        // Move to the target position
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);
        cd.faceUp = true; // Make it face up
        // Set the depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    // Arranges all the cards of the drawPile to show how many are left
    void UpdateDrawPile()
    {
        CardProspector cd;
        // Go through all the cards of the drawPile
        for(int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            // Position it correctly with the layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false; // Make them all face down
            cd.state = CardState.drawpile;
            // Set depth sorting
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    // Return true if the two cards are adjacent in rank (A & K wrap around)
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        // If either card is face-down, it's not adjacent.
        if (!c0.faceUp || !c1.faceUp) return(false);
        // If they are 1 apart, they are adjacent
        if (Mathf.Abs(c0.rank - c1.rank) == 1) return (true);
        // If one is A and the other King, they're adjacent
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);
        // Otherwise, return false
        return (false);
    }

    // This turns cards in the Mine face-up or face-down
    void SetTableauFaces()
    {
        foreach(CardProspector cd in tableau)
        {
            bool fup = true; // Assume the card will be face-up
            foreach (CardProspector cover in cd.hiddenBy)
            {
                // If either of the covering cards are in the tableau
                if(cover.state == CardState.tableau)
                {
                    fup = false; // then this card is face-down
                }
            }
            cd.faceUp = fup; // Set the value on the card
        }
    }
}
