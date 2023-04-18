using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STEP_Parser
{
    public class StepPrinter
    {
        public StepPrinter(StepFile stepFile)
        {
            this.stepFile = stepFile;
            indentStringTerminator = "└──";
            indentStringSpaces = new string(' ', indentStringTerminator.Length);
        }

        private StepFile stepFile;
        private string indentStringTerminator;
        private string indentStringSpaces;
        private List<StringBuilder> treeStrings;
        private StringBuilder currentLine;

        public List<string> tree;

        //build a string representation of the tree
        public List<string> TreeToString()
        {
            treeStrings = new List<StringBuilder>();

            var topLevelIDs = stepFile.GetTopLevelEntities();

            foreach (int id in topLevelIDs)
            {
                PrintChildrenRecursive(id, 0);
            }

            for (int y = 1; y < treeStrings.Count; y++)
            {
                for (int x = 0; x < treeStrings[y].Length; x++)
                {
                    FillLineRecursive(y, x);
                }
            }

            tree = new List<string>();
            for (int y = 0; y < treeStrings.Count; y++)
            {
                tree.Add(treeStrings[y].ToString());
            }

            return tree;
        }


        public void FillLineRecursive(int y, int x)
        {
            if (treeStrings[y][x] == '└' && treeStrings[y - 1][x] == ' ')
            {
                treeStrings[y - 1][x] = '│';
                FillLineRecursive(y - 1, x);
            }
            else if (treeStrings[y][x] == '└' && treeStrings[y - 1][x] == '└')
            {
                treeStrings[y - 1][x] = '├';
                FillLineRecursive(y - 1, x);
            }
            else if (treeStrings[y][x] == '│' && treeStrings[y - 1][x] == '└')
            {
                treeStrings[y - 1][x] = '├';
                FillLineRecursive(y - 1, x);
            }
            else if (treeStrings[y][x] == '│' && treeStrings[y - 1][x] == ' ')
            {
                treeStrings[y - 1][x] = '│';
                FillLineRecursive(y - 1, x);
            }
        }


        public void PrintChildrenRecursive(int id, int depth)
        {
            if (id == -1) return;

            List<int> children = stepFile.GetChildren(id);
            currentLine = new StringBuilder();

            for (int i = 0; i < depth - 1; i++)
            {
                currentLine.Append(indentStringSpaces);
            }

            if (depth > 0)
            {
                currentLine.Append(indentStringTerminator);
            }

            currentLine.Append(stepFile.Entitys[id].entityID.ToString() + " " + stepFile.Entitys[id].type);
            treeStrings.Add(currentLine);

            foreach (int cid in children)
            {
                PrintChildrenRecursive(cid, depth + 1);
            }
        }


    }
}
