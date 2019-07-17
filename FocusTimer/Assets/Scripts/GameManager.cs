﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class GameManager : MonoBehaviour {
    //ブール値系
    bool isCounting = true;  //計測が開始しているか
    bool isPopup = false;  //ポップアップが表示されているか

    //表示用テキスト
    [SerializeField] Text timerTextHour;  //時
    [SerializeField] Text timerTextMinute;  //分
    [SerializeField] Text timerTextSecond;  //秒

    //ポップアップ時の表示用テキスト
    [SerializeField] Text popupTextHour;  //時
    [SerializeField] Text popupTextMinute;  //分
    [SerializeField] Text popupTextSecond;  //秒

    //一日の合計時間表示用テキスト
    [SerializeField] Text dayTextHour;  //時
    [SerializeField] Text dayTextMinute;  //分
    [SerializeField] Text dayTextSecond;  //秒

    //時間カウント用
    int hours;  //時
    int minutes;  //分
    int seconds;  //秒
    float time;  //現在のカウント時間（秒）

    //ゲームオブジェクト系
    [SerializeField] GameObject popup;  //ポップアップオブジェクト

    //パス系
    string basePath;  //ベースとなるパス
    string savePath;  //保存先のパス
    DirectoryInfo info;

    // Start is called before the first frame update
    void Start() {
        //エディタ上、アンドロイド、iOSそれぞれについて保存パスを取る
#if UNITY_EDITOR
        basePath = Application.dataPath;
        //FocusTimerフォルダがなければ作る
        if (!Directory.Exists(basePath + "/FocusTimer")) {
            Directory.CreateDirectory(basePath + "/../FocusTimer");
        }
        //DirectoryInfoを取る
        info = new DirectoryInfo(basePath + "/../FocusTimer");
        savePath = info.FullName + "/TimeHistory.txt";
#elif UNITY_ANDROID
        basePath = Application.persistentDataPath;
        savePath = basePath + "/../TimeHistory.txt";
#elif UNITY_IOS
        basePath = Application.persistentDataPath;
        info = new DirectoryInfo(basePath + "/FocusTimer");
        savePath = info.FullName + "/TimeHistory.txt";
#endif

        //1000行分の最初分の行を記入する
        using (StreamWriter writer = new StreamWriter(savePath)) {
            FileInfo info = new FileInfo(savePath);
            if (info.Length == 0) {
                for (int i = 0; i < 10; i++) {
                    writer.WriteLine(DateTime.Today.AddDays(i).Date);
                }
                writer.Close();
            }
        }
    }

    // Update is called once per frame
    void Update() {
        //カウントフラグがオンの時のみカウントを有効にする
        if (isCounting) {
            time += Time.deltaTime;

            //秒の計算
            seconds = (int)Mathf.Floor(time) % 60;  //現在のカウント時間を60で割った余りが秒である

            //分の計算
            minutes = (int)Mathf.Floor(time) / 60;  //ここで分数を求める
            minutes %= 60;  //それを60で割った余りが分となる

            //時の計算
            hours = (int)Mathf.Floor(time) / 3600;  //3600で割った商が時間である
        }

        //時間の表示
        timerTextSecond.text = String.Format("{0:D2}", seconds);  //秒
        timerTextMinute.text = String.Format("{0:D2}", minutes);  //分
        timerTextHour.text = String.Format("{0:D2}", hours);  //時
        //ポップアップ用の時間
        popupTextSecond.text = String.Format("{0:D2}", seconds);  //秒
        popupTextMinute.text = String.Format("{0:D2}", minutes);  //分
        popupTextHour.text = String.Format("{0:D2}", hours);  //時

        //ポップアップの表示
        popup.SetActive(isPopup);
    }

    //再生ボタンを押した
    public void PressStart() {
        //カウント停止中の時
        if (!isCounting) {
            //カウント再開する
            isCounting = true;
        }
    }

    //一時停止ボタンを押した
    public void PressTemp() {
        //カウント中の時
        if (isCounting) {
            //一時停止する
            isCounting = false;
        }
    }

    //停止ボタンを押した
    public void PressStop() {
        //カウント中か否かにかかわらず、カウントを停止する
        isCounting = false;

        //ポップアップを表示する
        isPopup = true;
    }

    //キャンセルボタンを押した
    public void PressCansel() {
        //ポップアップ関連の操作
        PressPopUp();
    }

    //OKボタンを押した
    public void PressOK() {
        //これまでの時間を取得する
        FileStream fs_read = File.OpenRead(savePath);
        StreamReader reader = new StreamReader(fs_read);
        string total_time = null;
        while (true) {
            string read = reader.ReadLine();
            if (read == null) {
                break;
            }
            string[] splits = read.Split(' ');
            if (splits[0] == DateTime.Today.ToString().Split(' ')[0]) {
                total_time = splits[1];
            }
        }
        fs_read.Seek(0, SeekOrigin.Begin);  //読み込み位置初期化
        string all;
        all = reader.ReadToEnd();
        reader.Close();

        //取得したこれまでの時間を秒に変換する
        string[] time_splits = total_time.Split(':');
        int time_passed = 3600 * int.Parse(time_splits[0]) + 60 * int.Parse(time_splits[1]) + int.Parse(time_splits[2]);

        //★合計時間をテキストファイルに記録する
        int time_sum = time_passed + (int)Mathf.Floor(time);
        //合計時間を、00:00:00の形で表記する
        //秒の計算
        int seconds_sum = (int)Mathf.Floor(time_sum) % 60;  //現在のカウント時間を60で割った余りが秒である
        //分の計算
        int minutes_sum = (int)Mathf.Floor(time_sum) / 60;  //ここで分数を求める
        minutes %= 60;  //それを60で割った余りが分となる
        //時の計算
        int hours_sum = (int)Mathf.Floor(time_sum) / 3600;  //3600で割った商が時間である
        //文字列置換
        all = all.Replace(DateTime.Today.ToString().Split(' ')[0] + ' ' + total_time, DateTime.Today.ToString().Split(' ')[0] + ' ' + hours_sum.ToString() + ':' + minutes_sum.ToString() + ':' + seconds_sum.ToString());

        StreamWriter writer = new StreamWriter(savePath, false);
        writer.Write(all);
        writer.Close();

        //ポップアップ関連の操作
        PressPopUp();

        //★合計時間を表示する
        dayTextSecond.text = String.Format("{0:D2}", seconds_sum);  //秒
        dayTextMinute.text = String.Format("{0:D2}", minutes_sum);  //分
        dayTextHour.text = String.Format("{0:D2}", hours_sum);  //時
    }

    void PressPopUp() {
        //ポップアップを隠す
        isPopup = false;

        //時間をゼロにリセットする
        time = 0;
        hours = 0;
        minutes = 0;
        seconds = 0;
    }

    //日付が変わった時の処理
    void EndOfTheDay() {

    }
}