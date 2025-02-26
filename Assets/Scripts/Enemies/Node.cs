using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int position;
    public Node parent;
    public int gCost; //Cost from start to current node
    public int hCost; //Estimated cost from current node to end
    public int fCost{get {return gCost + hCost;}}// Total cost

    public Node(Vector2Int position){
        this.position = position;
    }
}
