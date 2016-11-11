using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is an enum, which defines a type of variable tht only has a few possible named values
// The CardState variable type has one of four values: drawpile, tableau, target, & discard
public enum CardState
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card {
     
    // This is how you use the enum CardState
    public CardState state = CardState.drawpile;
    // This hiddenBy list stores wich other cards will keep this one face down
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    // LayoutID matches this card to a Layout XML ID if it's a tableau card
    public int layoutID;
    // The SlowDef class stores information pulled in from they LayoutXML <slot>
    public SlotDef slotDef;

}
