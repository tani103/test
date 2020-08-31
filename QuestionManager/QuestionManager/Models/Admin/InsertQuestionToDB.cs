using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Configuration;
using Microsoft.VisualBasic.FileIO;
using QuestionManager.Models;


namespace QuestionManager.Models
{
    public class InsertQuestionToDB
    {
        /// <summary> 
        /// 終了後、画面に表示するメッセージ（エラーか、完了か）
        /// </summary>
        public string Message = string.Empty;

        /// <summary>
        /// CSVファイルから問題をDBに追加する
        /// </summary>
        /// <param name="uploadFile">読み込んだファイル</param>
        public string Insert(HttpPostedFileWrapper uploadFile)
        {
            if (uploadFile != null)
            {
                try
                {
                    // csvファイルならデータを読み込む
                    List<string[]> list = getData(uploadFile.InputStream);
                    // 問題をDBに追加する
                    setData(list);
                }
                catch
                {
                    if (Message != string.Empty)
                    {
                        // エラーメッセージ
                        return Message;
                    }
                    return "エラーが発生しました。";
                }

                if (Message != string.Empty)
                {
                    // エラーメッセージ
                    return Message;
                }
                return "問題を登録しました。";
            }

            return "ファイル形式が正しくありません。";
        }

        /// <summary>
        /// CSVファイルから追加する問題を取り出す
        /// </summary>
        /// <param name="file">読み込んだCSVファイル</param>
        /// <returns>ファイルから取り出した問題データ</returns>
        private List<string[]> getData(Stream file)
        {
            List<string[]> list = new List<string[]>();

            //ファイル読み込み
            TextFieldParser parser = new TextFieldParser(file, System.Text.Encoding.GetEncoding("Shift_JIS"));
            using (parser)
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(","); // 区切り文字はコンマ
                try
                {
                    // 一行目が項目名でなければ戻る
                    string line = parser.ReadLine();
                    if (line != "カテゴリ名,課題名,問題,正解")
                    {
                        Message = "一行目の項目名が「カテゴリ名,課題名,問題,正解」ではありません。";
                        throw new Exception();
                    }

                    // データをリストにする
                    while (!parser.EndOfData)
                    {
                        list.Add(parser.ReadFields());
                    }
                }
                catch
                {
                    Message = "ファイルを読み込めませんでした。";
                    throw;
                }
            }

            return list;
        }

        /// <summary>
        /// リストからDBにデータを追加する
        /// </summary>
        private void setData(List<string[]> list)
        {
            if (list.Count == 0)
            {
                Message = "ファイル内に追加する問題がありません。";
                throw new Exception();
            }

            using (QuestionEntities db = new QuestionEntities())
            {
                // csvファイルの形式チェック
                foreach (var item in list)
                {
                    if (item[0].Contains("\n") || item[1].Contains("\n"))
                    {
                        // 課題名で途中改行がある。
                        Message += String.Format("カテゴリ名か課題名に途中改行が入っています。\r\nエラーのため登録できませんでした。");
                        return;
                    }
                    else if (item[1] == AdminModels.FINAL_TEST_NAME)
                    {
                        // 課題名に「確認テスト」は指定できない
                        Message += String.Format("課題名には「" + AdminModels.FINAL_TEST_NAME + "」を指定できません。\r\nエラーのため登録できませんでした。");
                        return;
                    }
                }

                // すべてのカテゴリ
                var categoryDic = new Dictionary<string, int>();
                // 登録されているカテゴリを取得
                foreach (var category in db.Categories)
                {
                    categoryDic.Add(category.CategoryName, category.CategoryID);
                }

                // カテゴリIDの最大値
                int categoryCount = -1;
                if (categoryDic.Values.Count != 0)
                {
                    categoryCount = categoryDic.Values.Max();
                }

                // 全ての課題
                var dic = new Dictionary<string, int>();
                // 登録されている課題を取得
                foreach (var result in db.subjects.Where(x => x.Isdelete == false))
                {
                    dic.Add(result.SubjectName, result.SubjectID);
                }

                // 課題IDの最大値
                int subjectCount = -1;
                if (dic.Values.Count != 0)
                {
                    subjectCount = dic.Values.Max();
                }

                // 問題登録
                for (int i = 0; i < list.Count; i++)
                {
                    // カテゴリ名,課題名、問題、正解
                    string categoryName = list[i][0];
                    string subjectName = list[i][1];
                    string question = list[i][2];
                    string correct = list[i][3];
                    int timeLimit = 10;

                    if (!categoryDic.Keys.Contains(categoryName))
                    {
                        categoryCount++;
                        // 存在しないカテゴリ名である場合はカテゴリを登録する
                        Categories cat = Categories.CreateCategories(0, categoryName, false);
                        db.Categories.Add(cat);
                        db.SaveChanges();

                        // 確認テスト
                        subjectCount++;
                        int cid = db.Categories.Where(x => x.CategoryName == categoryName).Single().CategoryID;
                        subjects sub = subjects.CreateSubjects(0, AdminModels.FINAL_TEST_NAME, subjectCount, true, cid, AdminModels.FINAL_TEST_TIMELIMIT);
                        db.subjects.Add(sub);
                        db.SaveChanges();

                        // リストにも追加したカテゴリを登録
                        categoryDic[categoryName] = categoryCount;
                    }


                    // カテゴリ名からIDを取得
                    int categoryID = db.Categories.Where(x => x.CategoryName == categoryName).Single().CategoryID;

                    // 追加する課題ID
                    int subjectID;

                    // 存在しない課題名の場合、課題を登録する
                    // カテゴリが違う同一の名前の課題も登録する
                    if (!dic.Keys.Contains(subjectName) || db.subjects.Where(x => x.CategoryID == categoryID && x.SubjectName == subjectName).Count() == 0)
                    {
                        // DBに課題名追加(s_orderには最後尾の値を設定する)
                        subjectCount++;
                        subjects sub = subjects.CreateSubjects(0, subjectName, subjectCount, false, categoryID, timeLimit);
                        db.subjects.Add(sub);
                        db.SaveChanges();

                        // リストにも追加した課題を登録
                        dic[subjectName] = subjectCount;
                    }
                    else
                    {
                        subjectID = db.subjects.Where(x => x.SubjectName == subjectName).Single().SubjectID;

                        var subject = db.subjects.Where(x => x.SubjectID == subjectID).SingleOrDefault();
                        // 削除フラグがある場合、DBから削除して、新しく登録する
                        if (subject.Isdelete == true)
                        {
                            // 問題を消す
                            var q = db.questions.Where(x => x.SubjectID == subjectID);
                            foreach (var data in q)
                            {
                                db.questions.Remove(data);
                            }
                            // 受験者情報を消す
                            var u = db.MemberProgress.Where(x => x.SubjectID == subjectID);
                            foreach (var data in u)
                            {
                                db.MemberProgress.Remove(data);
                            }

                            // temporaryProgress
                            var temporaryProgress = db.TemporaryProgress.Where(x => x.SubjectID == subjectID);
                            foreach (var data in temporaryProgress)
                            {
                                db.TemporaryProgress.Remove(data);
                            }

                            // Historys
                            var Historys = db.Historys.Where(x => x.SubjectID == subjectID);
                            foreach (var data in Historys)
                            {
                                db.Historys.Remove(data);
                            }

                            // examResult
                            var examResult = db.ExamResult.Where(x => x.SubjectID == subjectID);
                            foreach (var data in examResult)
                            {
                                db.ExamResult.Remove(data);
                            }

                            db.SaveChanges();
                            db.subjects.Remove(subject);
                            db.SaveChanges();

                            // DBに課題名追加(s_orderには最後尾の値を設定する)
                            subjectCount++;
                            subjects sub = subjects.CreateSubjects(0, subjectName, subjectID, false, categoryID, timeLimit);
                            db.subjects.Add(sub);
                            db.SaveChanges();

                            // 課題ID登録
                            dic[subjectName] = subjectCount;
                        }
                    }

                    // データ登録
                    subjectID = db.subjects.Where(x => x.SubjectName == subjectName && x.CategoryID == categoryID).Single().SubjectID;
                    questions que = questions.CreateQuestions(0, subjectID, question, correct, false, null);
                    db.questions.Add(que);
                    db.SaveChanges();
                }
            }
        }
    }
}