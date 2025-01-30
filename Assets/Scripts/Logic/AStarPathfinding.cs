using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class AStarPathfinding {
                

    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos, List<Vector2Int> monsterView) {
        Node startNode = new Node(startPos);
        Node targetNode = new Node(targetPos);

        if (!TileManager.i.CheckTileStandable(targetPos)) {
            Debug.Log("Target position is not walkable. Finding alternative target.");
            targetNode = GetAlternativeTarget(targetNode, startPos); // プレイヤー位置を追加
            if (targetNode == null) {
                Debug.Log("No alternative target found.");
                return null;
            }
        }

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        openList.Add(startNode);

        int loopCount = 0;
        int maxLoopCount = 1000; // 最大ループ回数
        int maxOpenListSize = 100; // openListの最大サイズ

        while (openList.Count > 0) {
            loopCount++;
            if (loopCount >= maxLoopCount) { //無限ループ回避                
                Debug.Log("Loop count exceeded maximum limit. Pathfinding stopped.");
                break;
            }

            Node currentNode = openList[0]; //1回目はstartNodeを入れる
            for (int i = 1; i < openList.Count; i++) { //openListが2個以上あるときの処理
                if (openList[i].fCost < currentNode.fCost || openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost) { //openListのfCost, もしくはhCostがcurrentNodeよりも小さい場合は入れ替える
                    currentNode = openList[i];                    
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode); //currentNodeをopenListからclosedListへ移す

            if (currentNode.position == targetNode.position) { //currentNodeがゴールした場合は終了してパスを辿っていく
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbour in GetNeighbours(currentNode)) { //currentNodeの周囲８マスのNodeに対して処理を行う
                if (!TileManager.i.CheckMovableTile(currentNode.position, neighbour.position) || closedList.Contains(neighbour)) { //移動不可位置とclosedListのNodeは無視する                    
                    continue;
                }

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour); //totalCostを算出            
                if (newCostToNeighbour < neighbour.gCost || !openList.Any(node => node.position == neighbour.position)) { //neighbourのgCostがneighbourのtotalCostよりも大きい場合
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openList.Any(node => node.position == neighbour.position)) {
                        openList.Add(neighbour);
                    }

                    if (openList.Count > maxOpenListSize) {
                        Debug.Log("Open list size exceeded maximum limit. Pathfinding stopped.");
                        return null;
                    }
                }
            }
        }

        Debug.Log("Path not found.");
        return null;
    }

    //ゴールから一つずつNodeのparentを辿っていく
    private List<Vector2Int> RetracePath(Node startNode, Node endNode) {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        int loopCount = 0;
        while (currentNode != startNode) {
            loopCount++;
            if (loopCount >= 100) { //無限ループ回避                
                Debug.Log("無限ループ2");
                break;
            }

            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    private List<Node> GetNeighbours(Node node) {
        List<Node> neighbours = new List<Node>();
        foreach (var direction in DungeonConstants.EightDirections) {
            Vector2Int neighbourPos = node.position + DungeonConstants.ToVector2Int[direction];
            neighbours.Add(new Node(neighbourPos));
        }
        return neighbours;
    }

    private int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);
        return Mathf.Max(dstX, dstY);
    }

    private Node GetAlternativeTarget(Node targetNode, Vector2Int startPos) {
        // Implementation of GetAlternativeTarget
        // Find the nearest standable node to the original targetNode
        // Consider the player's position (startPos) in determining the nearest node

        List<Node> neighbours = GetNeighbours(targetNode);
        Node closestNode = null;
        int closestDistance = int.MaxValue;

        foreach (Node neighbour in neighbours) {
            if (TileManager.i.CheckTileStandable(neighbour.position)) {
                int distance = GetDistance(neighbour, new Node(startPos));
                if (distance < closestDistance) {
                    closestNode = neighbour;
                    closestDistance = distance;
                }
            }
        }

        return closestNode;
    }
}
