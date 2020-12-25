using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabN8OOP
{

    public abstract class CObserver
    {
        public abstract void onSubjectChanged(CSubject who);
    }

    public class CSubject
    {
        private List<CObserver> observers;

        public CSubject()
        {
            observers = new List<CObserver>();
        }

        public void addObserver(CObserver o)
        {
            observers.Add(o);
        }

        public void notifyEveryOne()
        {
            for (int i = 0; i < observers.Count; ++i)
            {
                observers[i].onSubjectChanged(this);
            }
        }
    }

    public class TreeViewObj : CObserver
    {
        private TreeView tree;
        private Repository repos;
        public TreeViewObj(TreeView treeView, Repository repository)
        {
            tree = treeView;
            repos = repository;
        }

        public override void onSubjectChanged(CSubject who)
        {
            tree.Nodes[0].Nodes.Clear();

            for (int i = 0; i < repos.getSize(); ++i)
            {
                if (!repos.isNull(i))
                {
                    processNode(tree.TopNode, repos.getObject(i));
                }
            }
            SetChecked();
        }

        private void processNode(TreeNode tn, CShapes o)
        {
            tn.Nodes.Add(new TreeNode(o.GetType().Name));
            if (o is Group)
            {
                for (int j = 0; j < ((Group)o).getCount(); ++j)
                {
                    processNode(tn.Nodes[tn.Nodes.Count - 1], ((Group)o).getGroups()[j]);
                }
            }

        }

        private void SetChecked()
        {
            int pos = 0;
            for (int i = 0; i < repos.getSize(); ++i)
            {
                if (!repos.isNull(i))
                {
                    if (repos.getObject(i).getSelected())
                        tree.TopNode.Nodes[pos++].Checked = true;
                    else tree.TopNode.Nodes[pos++].Checked = false;
                }
            }
        }

        public void CopyChecked()
        {
            int pos = 0;
            for (int i = 0; i < repos.getSize(); ++i)
            {
                if (!repos.isNull(i))
                {
                    if (tree.TopNode.Nodes[pos++].Checked)
                        repos.getObject(i).setSelected();
                    else repos.getObject(i).unSelected();
                }
            }
        }
    }

    public class Data : CSubject
    {
        protected CShapes[] arr;    // Массив элементов
        protected int size;   // Размер массива
        protected int count;  // Количество элементов
        public virtual CShapes createShape(string c)
        {
            return null;
        }

        public void loadShapes(string filename) // Загрузка фигур
        {
            arr = null;
            arr = new CShapes[10];
            StreamReader sr;
            using (sr = new StreamReader(filename)) // Открываем файл
            {
                count = Convert.ToInt32(sr.ReadLine()); // Считываем кол-во файлов
                if (count > size)   // Проверяем размер
                {
                    size = count + 1;
                    CShapes[] tmp = new CShapes[size];
                    arr = tmp;
                }
                for (int i = 0; i < count; ++i)
                {
                    arr[i] = createShape(sr.ReadLine());    // Создаем фигуру

                    if (arr[i] != null)
                        arr[i].load(sr);    // Загружаем фигуру
                }
            }
            sr.Close(); // Закрываем файл
            notifyEveryOne();
        }

        public void saveShapes(string filename) // Сохранение фигур
        {
            StreamWriter sw;
            using (sw = new StreamWriter(filename)) // Открываем файл
            {
                sw.WriteLine(count);    // Записываем кол-во файлов
                for (int i = 0; i < size; ++i)
                {
                    if (arr[i] != null)
                        arr[i].save(sw);    // Сохраняем фигуру
                }
            }
            sw.Close(); // Закрываем файл
        }
    }


    public class Repository : Data   // Хранилище
    {
        public Repository() // Конструктор
        {
            size = 0;
            count = 0;
            arr = new CShapes[size];
        }
        public Repository(int size) // Конструктор
        {
            this.size = size;
            count = 0;
            arr = new CShapes[size];
        }
        ~Repository()   // Деструктор
        {
            for (int i = 0; i < size; ++i)
            {
                if (!isNull(i))
                    arr[i] = null;
            }
        }
        public void delObject(int pos)  // Удаление объекта
        {
            arr[pos] = null;
            count--;
            notifyEveryOne();
        }
        public CShapes addObject(CShapes point) // Добавление элемента
        {
            int pos = 0;
            while (!isNull(pos) && pos < size)
            {
                pos++;
            }
            if (pos >= size - 1)
            {
                size = pos + 1;
                CShapes[] tmp = new CShapes[size];
                for (int i = 0; i < size - 1; ++i)
                {
                    tmp[i] = arr[i];
                }
                arr = tmp;
            }
            arr[pos] = point;
            count++;
            notifyEveryOne();
            return arr[pos];
        }
        public void setObject(int pos, CShapes point) // Изменение элемента
        {
            if (pos >= size)
            {
                int oldsize = size;
                size = pos + 1;
                CShapes[] tmp = new CShapes[size];
                for (int i = 0; i < size - 1; ++i)
                {
                    tmp[i] = arr[i];
                }
                arr = tmp;
                count++;
            }
            arr[pos] = point;
            notifyEveryOne();
        }
        public CShapes getObject(int pos)    // Получение элемента
        {
            return arr[pos];
        }
        public int getCount()   // Получение количества объектов
        {
            return count;
        }
        public int getSize()    // Получение размера хранилища
        {
            return size;
        }
        public bool isNull(int pos) // Проверка наличия
        {
            if (arr[pos] == null)
                return true;
            return false;
        }

        public override CShapes createShape(string c)   // Создаем фигуру
        {
            CShapes cs = null;
            switch (c)
            {
                case "CCircle":
                    cs = new CCircle(0, 0, 0);
                    break;
                case "CSquare":
                    cs = new CSquare(0, 0, 0);
                    break;
                case "CTriangle":
                    cs = new CTriangle(0, 0, 0);
                    break;
                case "Group":
                    cs = new Group(1);
                    break;
            }
            return cs;
        }
    }
}