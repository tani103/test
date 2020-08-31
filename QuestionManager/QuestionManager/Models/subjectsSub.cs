using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuestionManager.Models
{
    public partial class subjects
    {
        public static subjects CreateSubjects(global::System.Int32 subjectID, global::System.String subjectName, global::System.Int32 subjectOrder, global::System.Boolean isDelete, global::System.Int32 categoryID, global::System.Int32 timeLimit)
        {
            subjects sub = new subjects();
            sub.SubjectID = subjectID;
            sub.SubjectName = subjectName;
            sub.SubjectOrder = subjectOrder;
            sub.Isdelete = isDelete;
            sub.CategoryID = categoryID;
            sub.TimeLimit = timeLimit;
            return sub;
        }

    }
}