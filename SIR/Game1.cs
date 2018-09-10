using ClosedXML.Excel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SIR
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        public static Random rand;

        private List<Cell> cells = new List<Cell>();
        KeyboardState OldKeyState;
        Texture2D tex;
        int susCount;
        int infCount;
        int recCount;
        int timeSteps;

        //Variables to store the numbers of each category for the spreadshirt
        List<int> totalSus = new List<int>();
        List<int> totalInf = new List<int>();
        List<int> totalRec = new List<int>();
        List<int> totalTime = new List<int>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 900;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            rand = new Random();
            tex = Content.Load<Texture2D>("tile7x7.png");
            font = Content.Load<SpriteFont>("gameFont");
            timeSteps = 0;

            //Get the map from the txt file and convert to array
            //Adapted from http://stackoverflow.com/questions/18275494/xna-read-tile-maps-from-txt-file
            string[] mapData = File.ReadAllLines("Content/testMap2.txt");
            var width = mapData[0].Length;
            var height = mapData.Length;
            var tileData = new char[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    tileData[x, y] = mapData[y][x];
            }

            for (int y = 0; y < tileData.GetLength(1); y++ )
            {
                for (int x = 0; x < tileData.GetLength(0); x++ )
                {
                    Vector2 position = new Vector2(tex.Width * x, tex.Height * y);
                    Cell c = new Cell(tex, position, x, y);

                    //If the cell contains a 1 it is set to infected
                    if(tileData[x,y].Equals('1'))
                    {
                        c.state = CELL_STATE.INFECTED;

                    }
                    else
                    {
                        c.state = CELL_STATE.SUSCEPTIBLE;
                    }

                    cells.Add(c);
                }
            }

            foreach(Cell c in cells)
            {
                c.totalCells = cells;

                //Adapted from http://stackoverflow.com/questions/5640538/need-working-c-sharp-code-to-find-neighbors-of-an-element-in-a-2-dimentional-arr
                int refx = c.indexX, refy = c.indexY;
                var neighbours = from X in Enumerable.Range(0, tileData.GetLength(0)).Where(X => Math.Abs(X - refx) <= 1)
                                 from Y in Enumerable.Range(0, tileData.GetLength(1)).Where(Y => Math.Abs(Y - refy) <= 1)
                                 select new { X, Y };

                neighbours.ToList();
                List<Vector2> locations = new List<Vector2>();
                foreach (var n in neighbours)
                {
                    Vector2 neigh = new Vector2();

                    neigh.X = n.X;
                    neigh.Y = n.Y;
                    locations.Add(neigh);
                }
                c.findNeighbours(locations);
            }
            Count();
        }

   

        //These functions are required by XNA
        protected override void LoadContent()
        {

        }

        protected override void UnloadContent()
        {
          
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState NewKeyState = Keyboard.GetState();

            //Check if spacebar has been pressed, if so update the cells and cell counts
            if (NewKeyState.IsKeyDown(Keys.Space) && OldKeyState.IsKeyUp(Keys.Space))
            {
                foreach(Cell c in cells)
                {
                    c.update();
                }
                timeSteps++;
                Count();
                
            }

            //Export data to spreadsheet
            if (NewKeyState.IsKeyDown(Keys.Enter) && OldKeyState.IsKeyUp(Keys.Enter))
            {
                ExportData();
            }

            //For testing
            if (NewKeyState.IsKeyDown(Keys.NumPad1) && OldKeyState.IsKeyUp(Keys.NumPad1))
            {
                foreach(Cell c in cells)
                {
                    if(c.indexX == 47 && c.indexY == 27)
                    {

                        c.verifyTestThree();
                    }
                }
            }
            //Set keyboard state for next tick
            OldKeyState = NewKeyState;
            base.Update(gameTime);
        }

        //Used to collect the data for the HUD and spreadsheet
        private void Count()
        {
            susCount = 0;
            infCount = 0;
            recCount = 0;
            for(int i = 0; i < cells.Count; i++)
            {
                switch(cells[i].state)
                {
                    case CELL_STATE.SUSCEPTIBLE:
                        susCount++;
                        break;
                    case CELL_STATE.INFECTED:
                        infCount++;
                        break;
                    case CELL_STATE.RECOVERED:
                        recCount++;
                        break;
                    default:
                        break;
                };              
            }
            totalSus.Add(susCount);
            totalInf.Add(infCount);
            totalRec.Add(recCount);
            totalTime.Add(timeSteps);
        }


        public void ExportData()
        {
            //Create new Excel workbook
            var workBook = new XLWorkbook();
            var ws = workBook.Worksheets.Add("SIR Data");
            ws.Cell("A1").Value = "TimeSteps";
            ws.Cell("B1").Value = "Susceptible";
            ws.Cell("C1").Value = "Infected";
            ws.Cell("D1").Value = "Recovered";

            //Populate timestep data
            for (int i = 0; i < totalTime.Count; i++ )
            {
                string cell = "A" + (i+2);
                ws.Cell(cell).Value = totalTime[i];
            }

            //Populate Susceptible data
            for (int i = 0; i < totalSus.Count; i++)
            {
                string cell = "B" + (i + 2);
                ws.Cell(cell).Value = totalSus[i];
            }

            //Populate Infected data
            for (int i = 0; i < totalInf.Count; i++)
            {
                string cell = "C" + (i + 2);
                ws.Cell(cell).Value = totalInf[i];
            }

            //Populate Recovered data
            for (int i = 0; i < totalRec.Count; i++)
            {
                string cell = "D" + (i + 2);
                ws.Cell(cell).Value = totalRec[i];
            }
            workBook.SaveAs("SIR_" + DateTime.Now.ToString("dd-MM-yyyy-HH_mm", CultureInfo.InvariantCulture) + ".xlsx");
        }

        //Co-ordinates the rendering
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            // TODO: Add your drawing code here
            foreach(Cell c in cells)
            {
                c.draw(spriteBatch);
            }
            DrawText();
            spriteBatch.End();
            base.Draw(gameTime);
        }


        private void DrawText()
        {
            spriteBatch.DrawString(font, "Time: " + timeSteps, new Vector2(20, 850), Color.White,0f,new Vector2(),0.3f,SpriteEffects.None,0f);
            spriteBatch.DrawString(font, "Susceptible: " + susCount, new Vector2(140, 850), Color.White, 0f, new Vector2(), 0.3f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, "Infected: " + infCount, new Vector2(400, 850), Color.White, 0f, new Vector2(), 0.3f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, "Recovered: " + recCount, new Vector2(650, 850), Color.White, 0f, new Vector2(), 0.3f, SpriteEffects.None, 0f);
        }
    }
}
