using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace SIR
{
    class Cell
    {
        //Location of the cell
        private Vector2 position;
        public int indexX;
        public int indexY;
        Vector2 index;
        //List of neighbours
        public List<Vector2> neighbours;
        //The current state of the cell
        public CELL_STATE state;
        Color col;
        public List<Cell> totalCells;
        public List<Cell> nCells;
        Texture2D tex;

        public Cell(Texture2D t, Vector2 pos, int x, int y)
        {
            nCells = new List<Cell>();
            position = pos;
            indexX = x;
            indexY = y;
            index = new Vector2(x, y);
            tex = t;
            neighbours = new List<Vector2>();
        }

        //Iterate through all of the cells and find the neighbours which match the vector list
        public void findNeighbours(List<Vector2> n)
        {
            foreach (Cell c in totalCells)
            {
                if (this != c)
                {
                    foreach (Vector2 v in n)
                    {
                        if (v == c.index)
                        {
                            nCells.Add(c);
                        }
                    }
                }
            }
        }

        //The SIR logic takes place here
        public void update()
        {
            //First step is to count the affected neighbours
            int infCount = getInfCount();

            //Reset the infection chance
            float infectionChance = 0;

            //The recovered state is immune, therefore nothing happens if it already is Recovered
            if(this.state == CELL_STATE.RECOVERED)
            {
                this.state = CELL_STATE.RECOVERED;
            }
            else if(this.state == CELL_STATE.INFECTED)
            {
                //Get random number below 1
                float recoveryChance = (float)Game1.rand.Next(100) / 100;
                //original was 0.15
                float recoveryRate = 0.15f;
                //If the number is below the recovery rate the cell recovers
                if(recoveryChance < recoveryRate)
                {
                    this.state = CELL_STATE.RECOVERED;
                }
            }
            else if(this.state == CELL_STATE.SUSCEPTIBLE)
            {
                //If there are infected neighbours
                if(infCount > 0)
                {
                    //The infection chance is weighted by the number of infected neighbours and multiplied by constant number
                    infectionChance = (float)infCount * 0.125f;
                    //original was 0.3
                    float infectionRate = 0.3f;
                    //If the number is above the threshold the cell is infected
                    if(infectionChance >= 0.9)
                    {
                        this.state = CELL_STATE.INFECTED;
                    }
                    else
                    {
                        //If there are not enough infected neigbours a random chance is introduced.
                        infectionChance = (float)Game1.rand.Next(100) / 100;
                        if (infectionChance < infectionRate)
                        {
                            this.state = CELL_STATE.INFECTED;
                        }
                        else
                        {
                            this.state = CELL_STATE.SUSCEPTIBLE;
                        }
                    }
                }
            }
            else
            {
                this.state = CELL_STATE.SUSCEPTIBLE;
            }

        }

        private int getInfCount()
        {
            int count = 0;
            foreach (Cell c in nCells)
            {
                if (c.state == CELL_STATE.INFECTED)
                {
                    count++;
                }
            }
            return count;
        }

        public void draw(SpriteBatch s)
        {
            //Colour the cell based on the state
            if (state == CELL_STATE.INFECTED)
            {
                col = Color.Red;
            }
            else if(state == CELL_STATE.RECOVERED)
            {
                col = Color.Green;
            }
            else
            {
                col = Color.White;
            }
            s.Draw(tex, new Rectangle(indexX * 8, indexY * 8, tex.Width, tex.Height), col);
        }

        //Tests for verification
        public void verifyTestOne()
        {
            foreach(Cell c in nCells)
            {
                if(c.state == CELL_STATE.SUSCEPTIBLE)
                {
                    c.state = CELL_STATE.INFECTED;
                }
                
            }
            Console.WriteLine(nCells.Count);
        }

        public void verifyTestTwo()
        {
            switch(this.state)
            {
                case CELL_STATE.SUSCEPTIBLE:
                    state = CELL_STATE.INFECTED;
                    break;
                case CELL_STATE.INFECTED:
                    state = CELL_STATE.RECOVERED;
                    break;
                case CELL_STATE.RECOVERED:
                    state = CELL_STATE.SUSCEPTIBLE;
                    break;
            };
        }

        public void verifyTestThree()
        {
            foreach(Cell c in nCells)
            {
                int rand = Game1.rand.Next(100);
                if(rand > 50)
                {
                    c.state = CELL_STATE.INFECTED;
                }
            }
        }
    }

    public enum CELL_STATE
    {
       SUSCEPTIBLE, INFECTED, RECOVERED
    }  
}
