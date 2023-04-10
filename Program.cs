using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

using static System.Console;
using static State;
using static GameStatus;
using static System.Math;

enum State{Empty, Falling, Fixed};
enum GameStatus{Intro, Game, Gameover};

namespace project
{
    class Position{
        public int x, y;
        public Position(int x, int y){ this.x = x; this.y = y;}
    }

    abstract class Tetromino{
        public abstract Brush brush {get;}
        protected Position[] defaultOrientation; //The first Position in one orientation is the pivot
        public int numOfOrientations;
        public Position[] orientation(int num){
            var orientation = new Position[4];
            for(int i = 0; i < 4; ++i){
                orientation[i] = new Position(defaultOrientation[i].x,defaultOrientation[i].y);
            }
            for(int i = 0; i < num - 1; ++i){
                for(int j = 1; j < 4; ++j){
                    (orientation[j].x, orientation[j].y) = (-orientation[j].y, orientation[j].x);
                }
            }
            return orientation;
        }
        public bool orientationTransitionPathSweepCheck(int from, int to, Playfield playfield){
            Position[] fromOrientation = orientation(from);
            Position[] toOrientation = orientation(to);
            bool sweepCheck = true;
            for(int i = 1; i < 4; ++i){
                int incrementX = toOrientation[i].x - fromOrientation[i].x;
                int incrementY = toOrientation[i].y - fromOrientation[i].y;
                int xFirstDistance = 0;
                int yFirstDistance = 0;
                bool xFirstRotatable = true;
                bool yFirstRotatable = true;
                // check the rotation is within the grid
                int fromXBound = playfield.currentTetrominoPivotPosition.x + fromOrientation[i].x;
                int fromYBound = playfield.currentTetrominoPivotPosition.y + fromOrientation[i].y;
                int toXBound = playfield.currentTetrominoPivotPosition.x + toOrientation[i].x;
                int toYBound = playfield.currentTetrominoPivotPosition.y + toOrientation[i].y;
                if  ( 
                        (fromXBound >= 0 && fromXBound < Playfield.PlayFieldWidth) &&
                        (fromYBound >= 0 && fromYBound < Playfield.PlayFieldHeight) &&
                        (toXBound >= 0 && toXBound < Playfield.PlayFieldWidth) &&
                        (toYBound >= 0 && toYBound < Playfield.PlayFieldHeight)
                    ){
                        // calculate the total distance to pivot if going x direction first
                        if (incrementX != 0){
                            for (int j = Sign(incrementX); Abs(j) <= Abs(incrementX); j += Sign(incrementX)){
                                xFirstDistance += Abs(fromOrientation[i].x + j) + Abs(fromOrientation[i].y);
                                if (playfield.grid[playfield.currentTetrominoPivotPosition.x + fromOrientation[i].x + j, playfield.currentTetrominoPivotPosition.y + fromOrientation[i].y] == Fixed){
                                    xFirstRotatable = false;
                                    break;
                                }
                            }
                        }
                        if (xFirstRotatable == true && incrementY != 0){
                            for (int j = Sign(incrementY); Abs(j) <= Abs(incrementY); j += Sign(incrementY)){
                                xFirstDistance += Abs(fromOrientation[i].x + incrementX) + Abs(fromOrientation[i].y + j);
                                if (playfield.grid[playfield.currentTetrominoPivotPosition.x + fromOrientation[i].x + incrementX, playfield.currentTetrominoPivotPosition.y + fromOrientation[i].y + j] == Fixed){
                                    xFirstRotatable = false;
                                    break;
                                }
                            }
                        }
                        // calculate the total distance to pivot if going y direction first
                        if (incrementY != 0){
                            for (int j = Sign(incrementY); Abs(j) <= Abs(incrementY); j += Sign(incrementY)){
                                yFirstDistance += Abs(fromOrientation[i].x) + Abs(fromOrientation[i].y + j);
                                if (playfield.grid[playfield.currentTetrominoPivotPosition.x + fromOrientation[i].x, playfield.currentTetrominoPivotPosition.y + fromOrientation[i].y + j] == Fixed){
                                    yFirstRotatable = false;
                                    break;
                                }
                            }
                        }
                        
                        if(yFirstRotatable == true && incrementX != 0){
                            for (int j = Sign(incrementX); Abs(j) <= Abs(incrementX); j += Sign(incrementX)){
                                yFirstDistance += Abs(fromOrientation[i].x + j) + Abs(fromOrientation[i].y + incrementY);
                                if (playfield.grid[playfield.currentTetrominoPivotPosition.x + fromOrientation[i].x + j, playfield.currentTetrominoPivotPosition.y + fromOrientation[i].y + incrementY] == Fixed){
                                    yFirstRotatable = false;
                                    break;
                                }
                            }
                        }
                    }
                else{
                    xFirstRotatable = false;
                    yFirstRotatable = false;
                }
                // check that each block is rotatable
                if  (
                    (xFirstDistance >= yFirstDistance && xFirstRotatable == true) ||
                    (yFirstDistance >= xFirstDistance && yFirstRotatable == true)
                    ){
                    continue;
                }
                else{
                    return false;
                }
            }
            return sweepCheck;
        }
        public abstract float[] centreRelativeToPivot {get;}
        public void orientationTransition(Playfield playfield, KeyEventArgs e){
            int from = playfield.currentTetrominoOrientation;
            int to;
            if (e.KeyCode == Keys.D){
                to = from % numOfOrientations + 1;
                if (orientationTransitionPathSweepCheck(from, to, playfield)){
                    playfield.changeCurrentTetrominoOrientation(to);
                }
            }
            else if (e.KeyCode == Keys.A){
                to = (from + numOfOrientations - 2) % numOfOrientations + 1;
                if (orientationTransitionPathSweepCheck(from, to, playfield)){
                    playfield.changeCurrentTetrominoOrientation(to);
                }
            }
        }
    }

    class I: Tetromino{
        // I's pivot is as the following
        // IPII
        public I(){
            defaultOrientation = new Position[4]{new Position(0, 0), new Position(-1, 0), new Position(1, 0), new Position(2, 0)};
            numOfOrientations = 2;
        }
        public override Brush brush {
            get{
                return Brushes.LightSkyBlue;
            }
        }
        public override float[] centreRelativeToPivot {
            get{
                return new float[3]{0, (float)0.5, 0};
            }
        }
    }

    class J: Tetromino{
        // J's pivot is as the following
        // J
        // PJJ
        public J(){
            defaultOrientation = new Position[4]{new Position(0, 0), new Position(0, -1), new Position(1, 0), new Position(2, 0)};
            numOfOrientations = 4;
        }
        public override Brush brush {
            get{
                return Brushes.DarkBlue;
            }
        }
        public override float[] centreRelativeToPivot {
            get{
                return new float[5]{0, 1, (float)0.5, -1, (float)-0.5};
            }
        }
    }

    class L: Tetromino{
        // L's pivot is as the follwoing
        //   L
        // LLP
        public L(){
            defaultOrientation = new Position[4]{new Position(0, 0), new Position(-2, 0), new Position(-1, 0), new Position(0, -1)};
            numOfOrientations = 4;
        }
        public override Brush brush {
            get{
                return Brushes.Orange;
            }
        }
        public override float[] centreRelativeToPivot {
            get{
                return new float[5]{0, -1, (float)0.5, 1, (float)-0.5};
            }
        }
    }

    class O: Tetromino{
        // O's pivot is as the following
        // OO
        // PO
        public O(){
            defaultOrientation = new Position[4]{new Position(0, 0), new Position(0, -1), new Position(1, -1), new Position(1, 0)};
            numOfOrientations = 1;
        }
        public override Brush brush {
            get{
                return Brushes.Yellow;
            }
        }
        public override float[] centreRelativeToPivot {
            get{
                return new float[2]{0, (float)0.5};
            }
        }
    }

    class S: Tetromino{
        // S's pivot is as the following
        //  SS
        // SP
        public S(){
            defaultOrientation = new Position[4]{new Position(0, 0), new Position(-1, 0), new Position(0, -1), new Position(1, -1)};
            numOfOrientations = 2;
        }
        public override Brush brush {
            get{
                return Brushes.LawnGreen;
            }
        }
        public override float[] centreRelativeToPivot {
            get{
                return new float[3]{0, 0, (float)0.5};
            }
        }
    }

    class T: Tetromino{
        // T's pivot is as the follwoing
        //  T
        // TPT
        public T(){
            defaultOrientation = new Position[4]{new Position(0, 0), new Position(-1, 0), new Position(0, -1), new Position(1, 0)};
            numOfOrientations = 4;
        }
        public override Brush brush {
            get{
                return Brushes.Purple;
            }
        }
        public override float[] centreRelativeToPivot {
            get{
                return new float[5]{0, 0, (float)0.5, 0, (float)-0.5};
            }
        }
    }

    class Z: Tetromino{
        // Z's pivot is as the following
        // ZZ
        //  PZ
        public Z(){
            defaultOrientation = new Position[4]{new Position(0, 0), new Position(-1, -1), new Position(0, -1), new Position(1, 0)};
            numOfOrientations = 2;
        }
        public override Brush brush {
            get{
                return Brushes.Red;
            }
        }
        public override float[] centreRelativeToPivot {
            get{
                return new float[3]{0, 0, (float)-0.5};
            }
        }
    }



    class Playfield{
        public const int PlayFieldWidth = 10;
        public const int PlayFieldHeight = 20;
        public State[,] grid = new State[PlayFieldWidth, PlayFieldHeight];
        public int currentTetrominoOrientation = 1; // ranges from 1 to at most 4
        public int nextTetrominoOrientation = 1; // ranges from 1 to at most 4
        public Position currentTetrominoPivotPosition = new Position(4, 2);
        public Tetromino currentTetromino = newTetrominoRandomGenerator();
        public Tetromino nextTetromino = newTetrominoRandomGenerator();
        public Timer myTimer = new Timer();
        public delegate void Notify();
        public event Notify changed;
        public GameStatus gameStatus = Intro;
        public int score = 0;
        public int currentLevel = 1;
        public int lines = 0;

        public Playfield(){
            for (int x = 0; x < PlayFieldWidth; ++x){
                for (int y = 0; y < PlayFieldHeight; ++y){
                    grid[x, y] = Empty;
                }
            }
            myTimer.Interval = 1000;
            myTimer.Tick += new EventHandler(onTick);
        }
        void onTick(object sender, EventArgs e){
            if  (
                currentTetrominoOneStepMovable(0, 1)
                ){
                ++currentTetrominoPivotPosition.y;
                changed();
            }
            else {
                fixCurrentTetrominoAndClearLines();
                nextTurn();
            }
        }
        static Tetromino newTetrominoRandomGenerator(){
            Random ran = new Random();
            int tetrominoRepresentationNumber = ran.Next(1, 8);
            switch (tetrominoRepresentationNumber){
                case 1: return new I();
                case 2: return new J();
                case 3: return new L();
                case 4: return new O();
                case 5: return new S();
                case 6: return new T();
                case 7: return new Z();
                default: return null;
            }
        }
        public bool currentTetrominoOneStepMovable(int xDirection, int yDirection){
            foreach (Position p in currentTetromino.orientation(currentTetrominoOrientation)){
                int x = currentTetrominoPivotPosition.x + p.x + xDirection;
                int y = currentTetrominoPivotPosition.y + p.y + yDirection;
                if (
                    x < 0 ||
                    y < 0 ||
                    x >= PlayFieldWidth ||
                    y >= PlayFieldHeight ||
                    grid[x, y] == Fixed
                    ){
                    return false;
                }
            }
            return true;
        }
        public void nextTurn(){
            Random ran = new Random();

            currentTetromino = nextTetromino;
            currentTetrominoOrientation = nextTetrominoOrientation;
            currentTetrominoPivotPosition = new Position(4, 2);

            nextTetromino = newTetrominoRandomGenerator();
            nextTetrominoOrientation = ran.Next(1, nextTetromino.numOfOrientations + 1);
            changed();
        }
        public void restart(){
            grid = new State[PlayFieldWidth, PlayFieldHeight];
            nextTurn();
            changed();
        }
        public void fixCurrentTetrominoAndClearLines(){
            foreach (Position p in currentTetromino.orientation(currentTetrominoOrientation)){
                if (grid[currentTetrominoPivotPosition.x + p.x, currentTetrominoPivotPosition.y + p.y] == Fixed){
                    myTimer.Stop();
                    restart();
                    gameStatus = Gameover;
                    break;
                }
                else{
                    grid[currentTetrominoPivotPosition.x + p.x, currentTetrominoPivotPosition.y + p.y] = Fixed;
                }
            }
            // remeber the lines to be deleted
            List<int> linesToBeCleared = new List<int>();
            for (int y = PlayFieldHeight - 1; y >= 0 && linesToBeCleared.Count < 4; --y){
                bool doClear = true;
                for (int x = 0; x < PlayFieldWidth; ++x){
                    if(grid[x, y] != Fixed){
                        doClear = false;
                    }
                }
                if(doClear == true){
                    linesToBeCleared.Add(y);
                }
            }
            // compute the lines, scores, and levels
            lines += linesToBeCleared.Count;
            if (linesToBeCleared.Count > 0){
                score += currentLevel * (linesToBeCleared.Count - 1) * (linesToBeCleared.Count - 1) * 100 + 100;
            }
            currentLevel = (lines / 30 % 4 + 1) > 4? 4 : (lines / 30 % 4 + 1);
            myTimer.Interval = (int)(1000 * (1 - (currentLevel - 1) * 0.2));
            // created a new grid without the deleted lines
            var newGrid = new State[PlayFieldWidth, PlayFieldHeight];
            int yForNewGrid = PlayFieldHeight - 1;
            for (int y = PlayFieldHeight - 1; y >= 0; --y){
                if(!linesToBeCleared.Contains(y)){
                    for (int x = 0; x < PlayFieldWidth; ++x){
                        newGrid[x, yForNewGrid] = grid[x, y];
                    }
                    --yForNewGrid;
                }                
            }
            grid = newGrid;
            changed();
        }
        public void changeCurrentTetrominoPivot(int xDirection, int yDirection){
            currentTetrominoPivotPosition.x += xDirection;
            currentTetrominoPivotPosition.y += yDirection;
            changed();
        }
        public void changeCurrentTetrominoOrientation(int o){
            currentTetrominoOrientation = o;
            changed();
        }
    }

    class MyForm : Form
    {
        const int ScreenWidth = 450;
        const int ScreenHeight = 587;
        Playfield playfield = new Playfield();
        bool paused = false;
        Timer promptTimer = new Timer();
        Brush promptBrush = Brushes.LightGray;
        void onChange(){
            Invalidate();
        }
        public MyForm()
        {
            playfield.changed += onChange;
            ClientSize = new Size(ScreenWidth, ScreenHeight);
            StartPosition = FormStartPosition.CenterScreen;
            promptTimer.Interval = 300;
            promptTimer.Tick += new EventHandler(onPrompt);
            promptTimer.Start();
        }
        protected override void OnKeyDown(KeyEventArgs args)
        {
            switch(playfield.gameStatus){
                case Intro:
                    playfield.gameStatus = Game;
                    playfield.myTimer.Start();
                    promptTimer.Stop();
                    break;
                case Gameover:
                    playfield.gameStatus = Game;
                    playfield.myTimer.Start();
                    promptTimer.Stop();
                    break;
                case Game:
                    if (paused == true){
                        if (args.KeyCode == Keys.F2){
                            playfield.myTimer.Enabled = !playfield.myTimer.Enabled;
                            paused = false;
                        }
                        else if (args.KeyCode == Keys.F3){
                            Application.Exit();
                        }
                        else if (args.KeyCode == Keys.F1){
                            paused = false;
                            playfield.myTimer.Start();
                            playfield.restart();
                        }
            
                    }
                    else{
                        if  (
                            args.KeyCode == Keys.Left &&
                            playfield.currentTetrominoOneStepMovable(-1, 0)
                        )
                        {
                            playfield.changeCurrentTetrominoPivot(-1, 0);
                        }
                        
                        else if (
                                args.KeyCode == Keys.Right &&
                                playfield.currentTetrominoOneStepMovable(1, 0)
                                )
                        {
                            playfield.changeCurrentTetrominoPivot(1, 0);
                        }
                        else if (args.KeyCode == Keys.Down)
                        {
                            if  (
                                playfield.currentTetrominoOneStepMovable(0, 1)
                                ){
                                playfield.changeCurrentTetrominoPivot(0, 1);
                            }
                            else {
                                playfield.fixCurrentTetrominoAndClearLines();
                                playfield.nextTurn();
                            }
                            
                        }
                        else if (args.KeyCode == Keys.Space)
                        {
                            while(playfield.currentTetrominoOneStepMovable(0, 1)){
                                playfield.changeCurrentTetrominoPivot(0, 1);
                            }
                            playfield.fixCurrentTetrominoAndClearLines();
                            playfield.nextTurn();
                        }
                        else if (args.KeyCode == Keys.A || args.KeyCode == Keys.D){
                            playfield.currentTetromino.orientationTransition(playfield, args);
                        }
                        else if (args.KeyCode == Keys.F3){
                            Application.Exit();
                        }
                        else if (args.KeyCode == Keys.F2){
                            playfield.myTimer.Enabled = !playfield.myTimer.Enabled;
                            paused = true;
                        }
                        else if (args.KeyCode == Keys.F1){
                            playfield.restart();
                        }
                    }
                    break;
            }
        }
        void onPrompt(Object sender, EventArgs e){
            if (promptBrush == Brushes.LightGray){
                promptBrush = Brushes.Black;
            }
            else{
                promptBrush = Brushes.LightGray;
            }
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs args)
        {
            Graphics g = args.Graphics;

            switch (playfield.gameStatus){
                case Intro:
                    g.DrawString("Tetris", new Font(FontFamily.GenericMonospace, 30), Brushes.Black, new PointF(ScreenWidth / 2 - 90, ScreenHeight / 2 - 60));
                    g.DrawString("Press any key to start.", new Font(FontFamily.GenericMonospace, 10), promptBrush, new PointF(ScreenWidth / 2 - 100, ScreenHeight / 2));
                    break;
                case Gameover:
                    promptTimer.Start();
                    g.DrawString("Game Over", new Font(FontFamily.GenericMonospace, 30), Brushes.Black, new PointF(ScreenWidth / 2 - 120, ScreenHeight / 2 - 60));
                    g.DrawString("Press any key to restart.", new Font(FontFamily.GenericMonospace, 10), promptBrush, new PointF(ScreenWidth / 2 - 105, ScreenHeight / 2));
                    break;
                case Game:           
                    // draw the frame
                    g.DrawRectangle(new Pen(Color.Black, 3), new Rectangle(1, 1, 447, 584));
                    g.DrawLine(new Pen(Color.Black, 3), 295, 1, 295, 585);
                    //draw the next
                    g.DrawString("Next", new Font(FontFamily.GenericMonospace, 30), Brushes.Black, new PointF((float)317.5, 35));

                    Brush nextTetriminoBrush = playfield.nextTetromino.brush;
                    float centreRelativeToPivot = playfield.nextTetromino.centreRelativeToPivot[playfield.nextTetrominoOrientation];
                    foreach (Position p in playfield.nextTetromino.orientation(playfield.nextTetrominoOrientation)){
                        g.DrawRectangle(new Pen(Color.Black, 1), new Rectangle(357 + p.x * 29 - (int)(centreRelativeToPivot * 29), 140 + p.y * 29, 29, 29));
                        g.FillRectangle(nextTetriminoBrush, 358 + p.x * 29 - (int)(centreRelativeToPivot * 29), 141 + p.y * 29, 28, 28);
                    }
                    //draw the score
                    g.DrawString("Score", new Font(FontFamily.GenericMonospace, 30), Brushes.Black, new PointF((float)300, 220));
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    g.DrawString(playfield.score.ToString(), new Font(FontFamily.GenericMonospace, 15), Brushes.Black, new Rectangle(297, 260, 150, 30), sf);

                    //draw the lines
                    g.DrawString("Lines", new Font(FontFamily.GenericMonospace, 30), Brushes.Black, new PointF((float)300, 290));
                    g.DrawString(playfield.lines.ToString(), new Font(FontFamily.GenericMonospace, 15), Brushes.Black, new Rectangle(297, 330, 150, 30), sf);

                    //draw the level
                    g.DrawString("Level", new Font(FontFamily.GenericMonospace, 30), Brushes.Black, new PointF((float)300, 360));
                    g.DrawString(playfield.currentLevel.ToString(), new Font(FontFamily.GenericMonospace, 15), Brushes.Black, new Rectangle(297, 400, 150, 30), sf);
                    //draw the shortcut keys
                    g.DrawString("F1: Restart", new Font(FontFamily.GenericMonospace, 10), Brushes.Black, new PointF((float)300, 530));
                    g.DrawString("F2: Pause/Resume", new Font(FontFamily.GenericMonospace, 10), Brushes.Black, new PointF((float)300, 540));
                    g.DrawString("F3: Exit", new Font(FontFamily.GenericMonospace, 10), Brushes.Black, new PointF((float)300, 550));
                    // draw the fixed blocks
                    for (int y = 0; y < Playfield.PlayFieldHeight; ++y){
                        for (int x = 0; x < Playfield.PlayFieldWidth; ++x){
                            if (playfield.grid[x, y] == Fixed){
                                g.DrawRectangle(new Pen(Color.Black, 1), new Rectangle(3 + x * 29, 3 + y * 29, 29, 29));
                                g.FillRectangle(Brushes.LightGray, 4 + x * 29, 4 + y * 29, 28, 28);
                            }
                            
                        }
                    }
                    //draw the current falling tetrimino
                    Brush currentTetriminoBrush = playfield.currentTetromino.brush;
                    Position currentTetriminoPivot = playfield.currentTetrominoPivotPosition;
                    foreach (Position p in playfield.currentTetromino.orientation(playfield.currentTetrominoOrientation)){
                        g.DrawRectangle(new Pen(Color.Black, 1), new Rectangle(3 + (p.x + currentTetriminoPivot.x) * 29, 3 + (p.y + currentTetriminoPivot.y) * 29, 29, 29));
                        g.FillRectangle(currentTetriminoBrush, 4 + (p.x + currentTetriminoPivot.x) * 29, 4 + (p.y + currentTetriminoPivot.y) * 29, 28, 28);
                    }
                    break;
            }
        }
    }
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Form form = new MyForm();
            form.Text = "Tetris";
            Application.Run(form);
        }
    }
}
