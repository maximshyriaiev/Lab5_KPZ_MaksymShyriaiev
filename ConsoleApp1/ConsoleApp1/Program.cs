using System;
using System.Collections.Generic;

namespace ConsoleApp1
{

    // Перелік типів відображення елементу
    public enum DisplayType
    {
        Block,
        Inline
    }

    // Перелік типів закриття елементу
    public enum ClosingType
    {
        SingleTag,
        ClosingTag
    }

    // Абстрактний клас вузла розмітки
    public abstract class LightNode
    {
        public abstract string OuterHTML { get; }
        public abstract string InnerHTML { get; }

        // Шаблонний метод для хуків життєвого циклу
        public virtual void OnCreated()
        {
            Console.WriteLine("Елемент створено");
        }

        public virtual void OnInserted()
        {
            Console.WriteLine("Елемент вставлено в DOM");
        }

        public virtual void OnRemoved()
        {
            Console.WriteLine("Елемент видалено з DOM");
        }

        public virtual void OnStylesApplied()
        {
            Console.WriteLine("Стилі застосовано до елементу");
        }

        public virtual void OnClassListApplied()
        {
            Console.WriteLine("Класи застосовано до елементу");
        }

        public virtual void OnTextRendered()
        {
            Console.WriteLine("Текст відображено на елементі");
        }

        // Ітератор для перебору дочірніх елементів
        public virtual IEnumerable<LightNode> GetChildren()
        {
            yield break;
        }

        // Метод виконання команди
        public virtual void ExecuteCommand(Command command)
        {
            command.Execute(this);
        }

        // Відвідувач
        public abstract void Accept(Visitor visitor);
    }

    // Клас для текстового вузла розмітки
    public class LightTextNode : LightNode
    {
        private string text;
        public LightTextNode(string text)
        {
            this.text = text;
        }

        public override string OuterHTML => text;
        public override string InnerHTML => text;

        public override void Accept(Visitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // Клас для елемента розмітки
    public class LightElementNode : LightNode
    {
        public string TagName { get; }
        public DisplayType Display { get; }
        public ClosingType Closing { get; }
        public List<string> Classes { get; }
        public List<LightNode> Children { get; }

        public LightElementNode(string tagName, DisplayType display, ClosingType closing, List<string> classes, List<LightNode> children)
        {
            TagName = tagName;
            Display = display;
            Closing = closing;
            Classes = classes;
            Children = children;
        }

        public override string OuterHTML
        {
            get
            {
                string outerHtml = $"<{TagName}";
                foreach (var cls in Classes)
                {
                    outerHtml += $" class=\"{cls}\"";
                }
                if (Closing == ClosingType.SingleTag)
                {
                    outerHtml += " />";
                }
                else
                {
                    outerHtml += ">";
                    foreach (var child in Children)
                    {
                        outerHtml += child.OuterHTML;
                    }
                    outerHtml += $"</{TagName}>";
                }
                return outerHtml;
            }
        }

        public override string InnerHTML
        {
            get
            {
                string innerHtml = "";
                foreach (var child in Children)
                {
                    innerHtml += child.OuterHTML;
                }
                return innerHtml;
            }
        }

        public override IEnumerable<LightNode> GetChildren()
        {
            foreach (var child in Children)
            {
                yield return child;
            }
        }

        public override void ExecuteCommand(Command command)
        {
            base.ExecuteCommand(command);
            foreach (var child in Children)
            {
                child.ExecuteCommand(command);
            }
        }

        public override void Accept(Visitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in Children)
            {
                child.Accept(visitor);
            }
        }
    }

    // Команда
    public abstract class Command
    {
        public abstract void Execute(LightNode node);
    }

    public class AddClassCommand : Command
    {
        private readonly string _className;

        public AddClassCommand(string className)
        {
            _className = className;
        }

        public override void Execute(LightNode node)
        {
            if (node is LightElementNode element)
            {
                element.Classes.Add(_className);
                Console.WriteLine($"Додано клас {_className} до елементу {element.TagName}");
            }
        }
    }

    // Стейт
    public abstract class State
    {
        public abstract void Handle(LightNode node);
    }

    public class ActiveState : State
    {
        public override void Handle(LightNode node)
        {
            Console.WriteLine("Елемент активний");
        }
    }

    public class InactiveState : State
    {
        public override void Handle(LightNode node)
        {
            Console.WriteLine("Елемент неактивний");
        }
    }

    // Відвідувач
    public abstract class Visitor
    {
        public abstract void Visit(LightNode node);
    }

    public class TextVisitor : Visitor
    {
        public override void Visit(LightNode node)
        {
            if (node is LightTextNode)
            {
                Console.WriteLine("Відвідано текстовий вузол");
            }
            else if (node is LightElementNode)
            {
                Console.WriteLine("Відвідано елемент");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            // Створення елементів розмітки та використання функціоналу
            var paragraph = new LightElementNode("p", DisplayType.Block, ClosingType.ClosingTag, new List<string> { "paragraph" }, new List<LightNode>
            {
                new LightTextNode("Це перший абзац."),
                new LightTextNode("Це другий абзац.")
            });

            var listItems = new List<LightNode>
            {
                new LightTextNode("Пункт 1"),
                new LightTextNode("Пункт 2"),
                new LightTextNode("Пункт 3")
            };

            var unorderedList = new LightElementNode("ul", DisplayType.Block, ClosingType.ClosingTag, new List<string> { "list" }, listItems);

            // Використання команди для додавання класу
            var addClassCommand = new AddClassCommand("highlight");
            paragraph.ExecuteCommand(addClassCommand);

            // Відвідувач для текстового вузла
            var textVisitor = new TextVisitor();
            paragraph.Accept(textVisitor);

            // Використання стану
            var activeState = new ActiveState();
            activeState.Handle(paragraph);
        }
    }
}
