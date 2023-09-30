using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    public enum COLOR
    {
      EMPTY, //EMPTY = 0
      BLACK, //BLACK = 1
      WHITE  //WHITE = 2
    };

    const int WIDTH = 8;
    const int HEIGHT = 8;

    [SerializeField] GameObject boardDisplay = null;
    [SerializeField] GameObject blackObject = null;
    [SerializeField] GameObject whiteObject = null;
    [SerializeField] GameObject emptyObject = null;

    COLOR[,] board = new COLOR[WIDTH,HEIGHT]; //盤面
    public COLOR player = COLOR.BLACK;

    //勝敗を表示するテキスト
    [SerializeField] Text resultText = null;
    [SerializeField] Text currentText = null;

    //Retryボタン
    [SerializeField] GameObject retryButton = null;

    //StageSelectボタン
    [SerializeField] GameObject stageSelectButton = null;

    //SEObject
    [SerializeField] GameObject SEObject = null;

    // Start is called before the first frame update
    void Start()
    {
        Initialize(); //盤面の初期化
        ShowBoard(); //盤面の表示
    }

    //初期化の関数, 盤面の初期状態を設定
    public void Initialize()
    {
      resultText.text = "";
      retryButton.SetActive(false);
      stageSelectButton.SetActive(false);
      board = new COLOR[WIDTH, HEIGHT];
      board[3, 3] = COLOR.WHITE;
      board[3, 4] = COLOR.BLACK;
      board[4, 3] = COLOR.BLACK;
      board[4, 4] = COLOR.WHITE;
      player = COLOR.BLACK;
      ShowBoard();
    }

    void ShowBoard()
    {
      foreach (Transform child in boardDisplay.transform)
      {
        Destroy(child.gameObject); //削除
      }

      for(int v = 0; v < HEIGHT; v++) //垂直(vertical)
      {
        for(int h = 0; h < WIDTH; h++) //水平(horizontal)
        {
          // boardの色に合わせて適切なPrefabを取得
          GameObject piece = GetPrefab(board[h, v]);

          if(board[h,v] == COLOR.EMPTY)
          {
            //座標を一時的に保持
            int x = h;
            int y = v;

            //pieceにイベントを設定
            piece.GetComponent<Button>().onClick.AddListener(() => { PutStone(x + "," + y); });
          }
          //取得したPrefabをboardDisplayの子オブジェクトにする
              piece.transform.SetParent(boardDisplay.transform);
        }
      }

      CurrentCheck();
    }

    //色によって適切なprefabを取得して返す関数
    GameObject GetPrefab(COLOR color)
    {
      GameObject prefab;
      switch (color)
      {
           case COLOR.EMPTY:   //空欄の時
               prefab = Instantiate(emptyObject);
               break;
           case COLOR.BLACK:   //黒の時
               prefab = Instantiate(blackObject);
               break;
           case COLOR.WHITE:   //白の時
               prefab = Instantiate(whiteObject);
               break;
           default:            //それ以外の時(ここに入ることは想定していない)
               prefab = null;
               break;
      }
      return prefab;
    }

    //駒を置く関数
    public void PutStone(string position)
    {
      //positionをカンマで分ける
        int h = int.Parse(position.Split(',')[0]);
        int v = int.Parse(position.Split(',')[1]);

        SEObject.GetComponent<SEManager>().RingPutSE();
        
        ReverseAll(h, v);
        ShowBoard();
        //ひっくり返していれば相手の番, 駒の色を変更
        if(board[h,v] == player)
        {
          player = player == COLOR.BLACK ? COLOR.WHITE : COLOR.BLACK;
          if(CheckPass())
          {
            //相手がパスの場合, 駒の色を自分の色に変更
            player = player == COLOR.BLACK ? COLOR.WHITE : COLOR.BLACK;

            //自分もパスか否か判定
            if(CheckPass())
            {
                //自分もパスだった場合, 勝敗を判定
                CheckGame();
            }
          }
        }
    }

    void Reverse(int h, int v, int directionH, int directionV)
    {
      //確認する座標x, yを宣言
      int x = h + directionH, y = v + directionV;

      //挟んでいるか確認してひっくり返す
      while (x < WIDTH && x >= 0 && y < HEIGHT && y >= 0)
      {
        //自分の駒だった場合
         if (board[x, y] == player)
         {
             //ここにひっくり返す処理を書く
             //ひっくり返す
             int x2 = h + directionH, y2 = v + directionV;
             int count = 0; //カウント用の変数を追加
             while (!(x2 == x && y2 == y))
             {
               board[x2, y2] = player;
               x2 += directionH;
               y2 += directionV;
               count++;
             }

             //1つ以上ひっくり返した場合
             if(count > 0)
             {
               //駒を置く
               board[h,v] = player;
             }

             break;
         }
         //空欄だった場合
         else if (board[x, y] == COLOR.EMPTY)
         {
             //挟んでいないので処理を終える
             break;
         }

          //確認座標を次に進める
          x += directionH;
          y += directionV;
      }
    }

    void ReverseAll(int h, int v)
    {
        Reverse(h, v, 1, 0);  //右方向
        Reverse(h, v, -1, 0); //左方向
        Reverse(h, v, 0, -1); //上方向
        Reverse(h, v, 0, 1);  //下方向
        Reverse(h, v, 1, -1); //右上方向
        Reverse(h, v, -1, -1);//左上方向
        Reverse(h, v, 1, 1);  //右下方向
        Reverse(h, v, -1, 1); //左下方向
    }

    bool CheckPass()
    {
      for (int v = 0; v < HEIGHT; v++)
        {
            for (int h = 0; h < WIDTH; h++)
            {
                //board[h, v]が空欄の場合
                if (board[h, v] == COLOR.EMPTY)
                {
                    COLOR[,] boardTemp = new COLOR[WIDTH, HEIGHT]; //盤面保存用の変数を宣言
                    Array.Copy(board, boardTemp, board.Length); //盤面の状態を保存用変数に保存しておく
                    ReverseAll(h, v); //座標h,vに駒を置いたとしてひっくり返してみる

                    //ひっくり返せればboard[h, v]に駒が置かれている
                    if (board[h, v] == player)
                    {
                        //ひっくり返したのでパスではない
                        board = boardTemp; //盤面をもとに戻す
                        return false;
                    }
                }
            }
        }
        //1つもひっくり返せなかった場合パス
        return true;
    }

    void CheckGame()
    {
        int black = 0;
        int white = 0;

        //駒の数を数える
        for (int v = 0; v < HEIGHT; v++)
        {
            for (int h = 0; h < WIDTH; h++)
            {
                switch (board[h, v])
                {
                    case COLOR.BLACK:
                        black++; //黒をカウント
                        break;
                    case COLOR.WHITE:
                        white++; //白をカウント
                        break;
                    default:
                        break;
                }
            }
        }

        if (black > white)
        {
            resultText.text = "Black win";
        }
        else if (black < white)
        {
            resultText.text = "White win";
        }
        else
        {
            resultText.text = "Draw";
        }

        retryButton.SetActive(true);
        stageSelectButton.SetActive(true);
    }

    void CurrentCheck()
    {
      int black = 0;
      int white = 0;

      //駒の数を数える
      for (int v = 0; v < HEIGHT; v++)
      {
          for (int h = 0; h < WIDTH; h++)
          {
              switch (board[h, v])
              {
                  case COLOR.BLACK:
                      black++; //黒をカウント
                      break;
                  case COLOR.WHITE:
                      white++; //白をカウント
                      break;
                  default:
                      break;
              }
            }
        }

        currentText.text = "Black : " + black + "\nWhite : " + white;
    }

    public void onClickRetry()
    {
      Initialize();
    }

    public void onClickSelect()
    {
      SceneManager.LoadScene("Select");
    }

}
