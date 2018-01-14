using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace Quad_Tree
{
    public partial class Form1 : Form
    {
        struct Node
        {
            public string M;//M码
            public int depth;//深度
            public int value;//节点值
            public bool flag;
            public static bool operator ==(Node Node1, Node Node2)
            {
                return (Node1.value == Node2.value);
            }
            public static bool operator !=(Node operand1, Node operand2)
            {
                return (operand1.value != operand2.value);
            }
            public override string ToString()
            {
                return M + "\t" + value.ToString() + "\t" + depth.ToString();
            }
        }

        public Form1()
        {
            InitializeComponent();
        }
        
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] allLines = File.ReadAllLines(openFileDialog1.FileName);
                    int row, column;
                    row = column = allLines.Length;
                    Node[] Nodes = new Node[row * column];
                    int n=Convert.ToInt32(Math.Log(row,2));
                    int count = 0;
                    for(int i=0;i<row;i++)
                    {
                        string[] line = allLines[i].Split(',');
                        for(int j=0;j<column;j++)
                        {
                            Nodes[count].value = int.Parse(line[j].Trim());
                            Nodes[count].M = (int.Parse(Convert.ToString(i, 2)) * 2+ int.Parse(Convert.ToString(j, 2))).ToString().PadLeft(n,'0');//计算M码，M=2*i+j
                            Nodes[count].depth = n;
                            Nodes[count].flag = false;
                            count++;
                        }
                    }
                    originalData(Nodes, row, column);
                    Morton(Nodes,row,column);
                    Array.Sort(Nodes,(item1,item2)=>{return string.Compare(item1.M, item2.M);});//排序
                   
                    //从底向上(down-top)的合并
                    for (int depth = n; depth >= 1; depth--)
                    {
                        for (int i = 0; i < Math.Pow(4,depth-1); i++)
                        {
                            double t1=Math.Pow(4,n-depth+1);
                            double t2=Math.Pow(4,n-depth);
                            int index0=Convert.ToInt32(i*t1+0*t2);//左上角点的索引
                            int index1=Convert.ToInt32(i*t1+1*t2);//右上角
                            int index2=Convert.ToInt32(i*t1+2*t2);//左下角
                            int index3=Convert.ToInt32(i*t1+3*t2);//右下角
                            //检查四个相邻M码对应的属性值,如果相同，则合并为一个大块;否则，存储四个格网的参数值（M码、深度、属性值）
                            if (Nodes[index0] == Nodes[index1]&& Nodes[index0] == Nodes[index2]
                                && Nodes[index0] == Nodes[index3]&& isMerged(Nodes,index0,depth,n))
                            {
                                for (int j = 0; j < Math.Pow(4,n-depth+1); j++)
                                {
                                    string temp = Nodes[index0+ j].M;
                                    Nodes[index0 + j].M = temp.Remove(temp.Length - 1);
                                    Nodes[index0 + j].flag = true;
                                    Nodes[index0 + j].depth--;
                                }
                            }
                        }
                    }
                    Dictionary<int, Node> finalM = new Dictionary<int, Node>();
                    count=0;
                    for(int i=0;i<Nodes.Length;i++)
                    {
                        if(!finalM.Values.Contains(Nodes[i]))
                            finalM.Add(count++, Nodes[i]);
                    }
                    Code(finalM);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //判断是否合并完
        static bool isMerged(Node[] nodes,int startIndex,int depth,int n)
        {
            if (depth != n) //跳过最底层
            {
                for (int i = 0; i < Math.Pow(4, n - depth+1); i++)
                {
                    if (!nodes[startIndex + i].flag)
                        return false;
                }
                return true;
            }
            else
            {
                return true;
            }
        }
       
        //显示编码
        void Code(Dictionary<int,Node> result)
        {
            listBox1.Items.Clear();
            listBox1.Items.Add("地址   M码  节点值   深度值");
            for (int i = 0; i < result.Keys.Count; i++)
            {
                listBox1.Items.Add(i.ToString()+"\t"+result[i].ToString());
            }
        }
       
        //显示原始数据
        void originalData(Node[] nodes,int row,int column)
        {
            textBox2.Text = "";
            int count=0;
            for(int i=0;i<row;i++)
            {
                for(int j=0;j<column;j++)
                {
                    textBox1.Text += nodes[count++].value.ToString().PadLeft(3);
                }
                textBox1.Text += Environment.NewLine;
            }

        }

        //显示Morton码
        void Morton(Node[] nodes,int row,int column)
        {
            textBox2.Text = "";
            textBox2.Text += "      i";
            for(int i=0;i<column;i++)
            {
                textBox2.Text += " " + i.ToString().PadLeft(3);
            }
            textBox2.Text += Environment.NewLine + "  j    ";
            for (int j = 0; j < column; j++)
            {
                textBox2.Text += " " + Convert.ToString(j, 2).PadLeft(3);
            }
            textBox2.Text += Environment.NewLine;
            int count = 0;
            for (int i = 0; i < row; i++)
            {
                textBox2.Text += i.ToString().PadLeft(3) + " " + Convert.ToString(i, 2).PadLeft(3);
                for (int j = 0; j < column; j++)
                {
                    textBox2.Text += " " + nodes[count++].M;
                }
                textBox2.Text += Environment.NewLine;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
