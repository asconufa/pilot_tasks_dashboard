using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Ascon.Pilot.WebClient.Extensions;
using Microsoft.AspNetCore.Authorization;
using Ascon.Pilot.Core;
using Ascon.Pilot.Server.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ascon.Pilot.WebClient.Controllers
{
    /// <summary>
    /// Контроллер модели Home
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private static IDictionary<int, MType> _types;
        private static IServerApi _serverApi;
        private static List<DPerson> persons;
        private const int projectTypeId = 5;
        #region ActionResults
        /// <summary>
        /// Представление Index
        /// </summary>
        /// <returns>представление Index</returns>
        public IActionResult Index()
        {
            //return RedirectToAction("Index", "Files");
            return View("Index");
        }

        /// <summary>
        /// Представление Tasks
        /// </summary>
        /// <returns>представление Tasks</returns>
        public IActionResult Tasks(string filter, int stage)// Желательно отрефакторить
        {
            List<PilotTask> tasks = new List<PilotTask>();
            try
            {
                _types = ControllerContext.HttpContext.Session.GetMetatypes();
                _serverApi = ControllerContext.HttpContext.GetServerApi();
                persons = _serverApi.LoadPeople();
                //Прямой обход дерева заданий в глубину
                DObject rootTask = _serverApi.GetObjects(new[] { DObject.TaskRootId }).First();
                foreach (var folder in rootTask.Children) //Вытаскиваем каталоги №1
                {
                    Guid folderGuid = folder.ObjectId;
                    DObject folderObject = _serverApi.GetObjects(new[] { folderGuid }).First();
                    foreach (var childFolder in folderObject.Children) // Вытаскиваем каталоги №2
                    {
                        Guid childFolderGuid = childFolder.ObjectId;
                        DObject childFolderObject = _serverApi.GetObjects(new[] { childFolderGuid }).First();
                        foreach (var taskWorkflow in childFolderObject.Children)
                        {
                            Guid taskWorkflowGuid = taskWorkflow.ObjectId;
                            DObject taskWorkflowObject = _serverApi.GetObjects(new[] { taskWorkflowGuid }).First();
                            foreach (var taskStage in taskWorkflowObject.Children)
                            {
                                Guid taskStageGuid = taskStage.ObjectId;
                                DObject taskStageObject = _serverApi.GetObjects(new[] { taskStageGuid }).First();
                                foreach (var task in taskStageObject.Children) // Вытаскиваем задания
                                {
                                    Guid taskGuid = task.ObjectId;
                                    DObject taskObject = _serverApi.GetObjects(new[] { taskGuid }).First();

                                    var tsk = new PilotTask();
                                    tsk.Title = taskObject.GetTaskTitle();

                                    int initiatorPosId = taskObject.GetInitiatorPosition();
                                    int executorPositionId = taskObject.GetExecutorPosition();
                                    tsk.Initiator = SearchPersonByPositionId(persons, initiatorPosId);
                                    tsk.Executor = SearchPersonByPositionId(persons, executorPositionId);
                                    foreach (Guid attachment in taskObject.Relations.TaskInitiatorAttachments)
                                    {
                                        tsk.Attachments.Add(_serverApi.GetObjects(new[] {attachment}).First());
                                    }
                                    tsk.DateOfCompletion = GetDateOfCompletion(taskObject).ToLocalTime();
                                    tsk.DeadlineDate = taskObject.GetTaskDeadline().ToLocalTime();
                                    tsk.DateOfAssignment = taskObject.Created.ToLocalTime();
                                    tsk.State = (TaskState)taskObject.GetTaskState();
                                    tasks.Add(tsk);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return Content(e.Message);
                //return View("~/Views/Shared/Error.cshtml");
            }
            ViewBag.Projects = GetProjects();
            ViewBag.Persons = persons.Select(x => x.DisplayName).Distinct();
            ViewBag.OrgUnits = _serverApi.LoadOrganisationUnits().Where(x => !x.IsLastLeaf && x.Title != "Root").Select(x => x.Title);
            List<PilotTask> tsks;
            switch (filter)
            {
                case "allExpired":
                    ViewBag.PgHeader = "Все просроченные задания";
                    tsks = ShowAllExpiredTasks(tasks);
                    WriteReport(tsks);
                    return View(tsks);
                case "stage":
                    tsks = ShowTasksByProgress(tasks, stage);
                    WriteReport(tsks);
                    return View(tsks);
                default:
                { 
                    string proj = Request.Query.FirstOrDefault(p => p.Key == "projectName").Value;
                    bool selectExpired = Request.Query.FirstOrDefault(x => x.Key == "isExpired").Value == "True";
                    if (proj != null)
                        if (selectExpired)
                        {
                            ViewBag.PgHeader = "Просроченные задания по проекту " + proj;
                            tsks = ShowExpiredTasksByProject(tasks, proj);
                            WriteReport(tsks);
                            return View(tsks);
                        }
                        else
                        {
                            ViewBag.PgHeader = "Задания по проекту " + proj;
                            tsks = ShowExpiredTasksByProject(tasks, proj);
                            WriteReport(tsks);
                            return View(ShowTasksByProject(tasks, proj));
                        }
                    else
                    {
                        string personName = Request.Query.FirstOrDefault(x => x.Key == "executorName").Value;
                        bool selectExpiredByPerson =
                            Request.Query.FirstOrDefault(x => x.Key == "expiredByPerson").Value == "True";
                        if (personName != null)
                            if (selectExpiredByPerson)
                            {
                                ViewBag.PgHeader = "Просроченные задания исполнителя " + personName;
                                tsks = ShowExpiredTasksByPerson(tasks, personName);
                                WriteReport(tsks);
                                return View(tsks);
                            }
                            else
                            {
                                ViewBag.PgHeader = "Задания исполнителя " + personName;
                                tsks = ShowTasksByExecutor(tasks, personName);
                                WriteReport(tsks);
                                return View(tsks);
                            }
                        else
                        {
                            string unitName = Request.Query.FirstOrDefault(x => x.Key == "orgUnit").Value;
                            bool selectExpiredByUnit =
                                Request.Query.FirstOrDefault(x => x.Key == "expiredByOrgUnit").Value == "True";
                                if (unitName != null)
                                    if (selectExpiredByUnit)
                                    {
                                        ViewBag.PgHeader = "Просроченные задания подразделения " + unitName;
                                        tsks = ShowTasksByOrgUnit(ShowAllExpiredTasks(tasks), unitName);
                                        WriteReport(tsks);
                                        return View(tsks);
                                    }
                                    else
                                    {
                                        ViewBag.PgHeader = "Задания подразделения " + unitName;
                                        tsks = ShowTasksByOrgUnit(tasks, unitName);
                                        WriteReport(tsks);
                                        return View(tsks);
                                    }
                        }
                    }
                    ViewBag.PgHeader = "Задания";
                    WriteReport(tasks);
                    return View(tasks);
                }
            }
        }

        /// <summary>
        /// Представление Types
        /// </summary>
        /// <returns>представление Types</returns>
        public IActionResult Types()
        {
            return View(HttpContext.Session.GetMetatypes().Values);
        }

        /// <summary>
        /// Установление типа иконок файлов для папки id
        /// </summary>
        /// <param name="id">уникальный идентификатор папки</param>
        /// <returns>представления иконок файлов для разных типов файлов</returns>
        public IActionResult GetTypeIcon(int id)
        {
            const string svgContentType = "image/svg+xml";

            var mTypes = HttpContext.Session.GetMetatypes();
            if (mTypes.ContainsKey(id))
            {
                var mType = mTypes[id];
                if (mType.Icon != null)
                    return File(mType.Icon, svgContentType);
            }
            return File(Url.Content("~/images/file.svg"), svgContentType);
        }
        /// <summary>
        /// Сообщения об ошибках
        /// </summary>
        /// <param name="message">Текст сообщения об ошибке</param>
        /// <returns>Представление сообщения об ошибке</returns>
        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            return View(message);
        }
        
        public VirtualFileResult WriteCsv()
        {
            return File("Report.csv", "application/octet-stream", "Report.csv");
        }
        #endregion

        #region Filters
        private static string SearchPersonByPositionId(List<DPerson> persons, int positionId)
        {
            foreach (DPerson worker in persons)
            {
                MPosition position =
                    worker.Positions.Where(x => x.Position == positionId).FirstOrDefault();
                if (position != null)
                    return worker.DisplayName;
            }
            return string.Empty;
        }

       public List<PilotTask> ShowAllExpiredTasks(List<PilotTask> tasks)
        {
            return
                // Отбираем только те задания, дата вып-я которых > дедлайна или задание ещё не выполнено, но срок прошёл
                new List<PilotTask>(
                    tasks.Where(
                        x =>
                            x.DeadlineDate < x.DateOfCompletion ||
                            (x.DateOfCompletion == DateTime.MinValue.ToLocalTime() && DateTime.Now.ToLocalTime() > x.DeadlineDate)));
        }

        public List<PilotTask> ShowExpiredTasksByPerson(List<PilotTask> tasks, string personName)
        {
            return
                new List<PilotTask>(
                    tasks.Where(x => x.Executor == personName)
                        .Where(x => x.DeadlineDate < x.DateOfCompletion ||
                                    (x.DateOfCompletion == DateTime.MinValue.ToLocalTime() && DateTime.Now.ToLocalTime() > x.DeadlineDate)));
        }

        public List<PilotTask> ShowExpiredTasksByProject(List<PilotTask> tasks, string projectName) //Дописать проверки на тип
        {
            List<PilotTask> expired = new List<PilotTask>(ShowAllExpiredTasks(tasks).Where(x => x.Attachments.Count != 0));
            List<PilotTask> result = new List<PilotTask>();
            foreach (PilotTask task in expired)
            {
                foreach (DObject attach in task.Attachments)
                {
                    if (attach.GetTitle(_types[attach.TypeId]).Contains(projectName))
                        result.Add(task);
                    else
                    {
                        DObject parent = _serverApi.GetObjects(new[] { attach.ParentId }).First();
                        if (parent.GetTitle(_types[parent.TypeId]).Contains(projectName))
                            result.Add(task);
                        else
                        {
                            DObject grandparent = _serverApi.GetObjects(new[] { parent.ParentId }).First();
                            if (grandparent.GetTitle(_types[grandparent.TypeId]).Contains(projectName))
                                result.Add(task);
                        }
                    }
                }
            }
            return result;
        }

        public List<PilotTask> ShowTasksByProgress(List<PilotTask> tasks, int progress)
        {
            switch (progress)
            {
                case 1:
                    ViewBag.PgHeader = "Выданные задания";
                    break;
                case 2:
                    ViewBag.PgHeader = "Задания в работе";
                    break;
                case 3:
                    ViewBag.PgHeader = "Отозванные задания";
                    break;
                case 4:
                    ViewBag.PgHeader = "Ожидающие проверки";
                    break;
                case 5:
                    ViewBag.PgHeader = "Завершённые задания";
                    break;
            }
            return new List<PilotTask>(tasks.Where(x => x.State == (TaskState)progress));
        }

        public List<PilotTask> ShowTasksByExecutor(List<PilotTask> tasks, string personName)
        {
            return new List<PilotTask>(tasks.Where(x => x.Executor == personName));
        }

        public  List<PilotTask> ShowTasksByProject(List<PilotTask> tasks, string projectName) //Дописать проверки на тип
        {
            List<PilotTask> result = new List<PilotTask>();
            foreach (PilotTask task in new List<PilotTask>(tasks.Where(x => x.Attachments.Count != 0)))
            {
                foreach (var attach in task.Attachments)
                {
                    if (attach.GetTitle(_types[attach.TypeId]).Contains(projectName))
                        result.Add(task);
                    else
                    {
                        DObject parent = _serverApi.GetObjects(new[] { attach.ParentId }).First();
                        if (parent.GetTitle(_types[parent.TypeId]).Contains(projectName) && parent.TypeId == projectTypeId)
                            result.Add(task);
                        else
                        {
                            DObject grandparent = _serverApi.GetObjects(new[] { parent.ParentId }).First();
                            if (grandparent.GetTitle(_types[grandparent.TypeId]).Contains(projectName))
                                result.Add(task);
                        }
                    }
                }
            }
            return result;
        }

        public List<PilotTask> ShowTasksByOrgUnit(List<PilotTask> tasks, string unitName)
        {
            var unit = _serverApi.LoadOrganisationUnits().Where(x => x.Title == unitName).FirstOrDefault();
            if (unit != null)
            {
                var result = new List<PilotTask>();
                foreach (int child in unit.Children)
                {
                    result.AddRange(ShowTasksByExecutor(tasks, SearchPersonByPositionId(persons, child)));
                }
                return result;
            }
            return new List<PilotTask>();
        }
        #endregion

        #region Helpers
        private DateTime GetDateOfCompletion(DObject task)
        {
            DValue completionDate;
            return task.Attributes.TryGetValue(SystemAttributes.TASK_DATE_OF_COMPLETION, out completionDate)
                ? (DateTime)completionDate
                : DateTime.MinValue;
        }

        private void WriteReport(List<PilotTask> tasks)
        {
            FileStream stream = new FileStream("wwwroot/Report.csv", FileMode.Create);
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.WriteLine(string.Format("{0};{1};{2};{3};{4};", "Заголовок задания", "Инициатор", "Исполнитель",
                    "Срок", "Завершено"));
                foreach (PilotTask task in tasks)
                {
                    writer.Write(string.Format("{0};{1};{2};{3};", task.Title, task.Initiator, task.Executor,
                        task.DeadlineDate));
                    if (task.DateOfCompletion == DateTime.MinValue.ToLocalTime())
                        writer.WriteLine("Не завершено;");
                    else
                        writer.WriteLine(string.Format("{0};", task.DateOfCompletion));
                }
            }
        }

        private List<string> GetProjects()
        {
            var result = new List<string>();
            DObject rootObject = _serverApi.GetObjects(new[] {DObject.RootId}).First();
            _types = ControllerContext.HttpContext.Session.GetMetatypes();
            SearchForChildProjects(ref result, rootObject);
            return result;
        }

        private void SearchForChildProjects(ref List<string> result, DObject parentObject)
        {
            _types = ControllerContext.HttpContext.Session.GetMetatypes();
            var childProjects = parentObject.Children.Where(x => x.TypeId == projectTypeId).ToArray();
            DObject childObject;
            foreach (var child in childProjects)
            {
                childObject = _serverApi.GetObjects(new[] {child.ObjectId}).First();
                result.Add(childObject.GetTitle(_types[childObject.TypeId]));
            }
            childProjects = parentObject.Children.Where(x => x.TypeId != projectTypeId).ToArray();
            foreach (var child in childProjects)
            {
                childObject = _serverApi.GetObjects(new[] { child.ObjectId }).First();
                SearchForChildProjects(ref result, childObject);
            }
        }
        #endregion
    }
}
