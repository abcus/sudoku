using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku {
	class Program {

		static internal int[,] board = {
			{3, 0, 0, 4, 0, 0, 0, 0, 0},
			{0, 0, 8, 0, 0, 0, 0, 9, 2},
			{0, 7, 0, 0, 0, 0, 4, 5, 0},
			{0, 0, 7, 0, 0, 0, 0, 0, 0},
			{0, 0, 1, 9, 0, 0, 8, 0, 3},
			{0, 3, 0, 0, 4, 0, 0, 2, 0},
			{8, 2, 0, 0, 0, 5, 9, 0, 0},
			{0, 0, 0, 0, 0, 0, 6, 0, 0},
			{0, 0, 3, 0, 8, 7, 0, 0, 0}
		};

		internal static bool solved = false;

		static void Main(string[] args) {
			printBoard();
			solve();
		}

		static bool isFull() {
			for (int i = 0; i < 9; i++){
				for (int j = 0; j < 9; j++){
					if (board[i, j] == 0){
						return false;
					}
				}
			}
			return true;
		}

		static List<Move> moveGenerator() {
			List<Move> moveList = new List<Move>();

			for (int i = 0; i < 9; i++) {
				for (int j = 0; j < 9; j++) {

					// Generate moves for empty squares
					if (board[i, j] == 0){
						
						int[] numFrequency = new int[10];

						for (int k = 0; k < 9; k++){
							numFrequency[board[i, k]] ++;
						}
						for (int l = 0; l < 9; l++){
							numFrequency[board[l, j]] ++;
						}
						for (int m = 0; m < 3; m++){
							for (int n = 0; n < 3; n++){
								numFrequency[board[i/3 * 3 + m, j/3 * 3 + n]] ++;
							}
						}
						int count = 0;
						for (int o = 1; o < 10; o++){
							if (numFrequency[o] == 0) count ++;
						}

						for (int p = 1; p <10; p++){
							int frequency = numFrequency[p];
							Debug.Assert(frequency >= 0 && frequency <= 3);
							if (frequency == 0){
								moveList.Add(new Move(p, i, j, 11-count));
							}
						}
					}
				}
			}

			for (int q=0; q < moveList.Count(); q++) {
				int bestScore = -1;
				int bestPosition = -1;
				for (int r = q; r < moveList.Count(); r++){
					if (moveList[r].score > bestScore){
						bestScore = moveList[r].score;
						bestPosition = r;
					}
				}
				Move temp = moveList[q];
				moveList[q] = moveList[bestPosition];
				moveList[bestPosition] = temp;
			}


			return moveList;
		}

		static void solve() {
			if (isFull()){
				solved = true;
				printBoard();
				return;
			}
			List<Move> moveList = moveGenerator();
			if (moveList.Count != 0){
				foreach (Move move in moveList) {
					
					board[move.locationX, move.locationY] = move.number; 
					solve();
					board[move.locationX, move.locationY] = 0; 

					if (solved){
						return;
					}
				}	
			}
		}

		static void printBoard() {
			for (int i = 0; i < 10; i++) {
				Console.WriteLine(i != 0 ? i < 9 ? i % 3 == 0 ? "╠═══╪═══╪═══╬═══╪═══╪═══╬═══╪═══╪═══╣" : "╟───┼───┼───╫───┼───┼───╫───┼───┼───╢" : "╚═══╧═══╧═══╩═══╧═══╧═══╩═══╧═══╧═══╝" : "╔═══╤═══╤═══╦═══╤═══╤═══╦═══╤═══╤═══╗");
				for (int j = 0; j < 10; j++) {
					Console.Write(i == 9 ? "" : j == 0 ? "║ " : (board[i, (j - 1)] != 0 ? board[i, (j - 1)].ToString() : " ") + (j % 3 == 0 ? " ║ " : " │ ") + (j == 9 ? "\n":""));
				}
			}
		}
	}

	struct Move {
		internal int number;
		internal int locationX;
		internal int locationY;
		internal int score;
		public Move(int number, int locationX, int locationY, int score) {
			this.number = number;
			this.locationX = locationX;
			this.locationY = locationY;
			this.score = score;
		}

		public override string ToString() {
			string move = "(" + locationX + ", " + locationY + "): " + number + "   (" + score + ")";
			return move;
		}
	}
}
