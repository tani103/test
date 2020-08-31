using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuestionManager.Models
{
    public partial class questions
    {
        public static questions CreateQuestions(global::System.Int32 questionID, global::System.Int32 subjectID, global::System.String question, global::System.String answer, global::System.Boolean isDelete, global::System.String picturePath)
        {
            questions que = new questions();
            que.QuestionID = questionID;
            que.SubjectID = subjectID;
            que.question = question;
            que.Answer = answer;
            que.Isdelete = isDelete;
            return que;
        }
    }
}