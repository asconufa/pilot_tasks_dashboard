using System;
using System.Text;
using Ascon.Pilot.Core;
using System.Collections.Generic;

namespace Ascon.Pilot.WebClient
{
    /*Модель представления для контрольной панели руководителя. Писалась до изучения моделей представления студентом-быдлокодером*/
    public class PilotTask
    {
        public string Title { get; set; }
        public string Initiator { get; set; }
        public string Executor { get; set; }
        public DateTime DeadlineDate { get; set; }
        public DateTime DateOfAssignment { get; set; }
        public List<DObject> Attachments { get; set; }
        public DateTime? DateOfCompletion { get; set; }
        public TaskState State { get; set; }
        public PilotTask()
        {
            Attachments = new List<DObject>();
        }
        public Guid InitiatorId { get; set; } 
        public Guid ExecutorId { get; set; }
        public override string ToString()
        {
            StringBuilder res = new StringBuilder(Title);
            res.Append("; Инициатор: ");
            res.Append(Initiator);
            res.Append("; Исполнитель: ");
            res.Append(Executor);
            res.Append("; Выдано: ");
            res.Append(DateOfAssignment);
            res.Append("; Срок: ");
            res.Append(DeadlineDate);
            if (DateOfCompletion != DateTime.MinValue.ToLocalTime())
            {
                res.Append("; Время выполнения: ");
                res.Append(DateOfCompletion);
            }
            return res.ToString();
        }
    }
    public enum TaskState
    {
        None,
        Assigned,
        InProgress,
        Revoked,
        OnValidation,
        Completed
    }
}
