using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//Sudoku Solver

// Move generation:
// 1) raw move generation
//       moves with score 10 mean that the square must have that value (e.g. 7); if it has any other value (e.g. 8) then there would be two of that other value (8) in a unit(s)
// 2) scans every unit (row, column, block) to see if a certain value (e.g. 7) is possible in only one square of that unit
//   If so, then that square must contain that value, so eliminates rest of possible values for that square
//		moves with score 100 mean that the square must have that value (e.g. 7); if it has another value (e.g. 8) then there would be none of that value (7) in a unit(s)

// Cuts off search if:
// 1) Any square has 0 possible values (which means that the position must be incorrect)
// 2) Any unit has no possibility of having a certain value (e.g. 7)

// Heuristics:
// When making moves, choose squares with the fewest possible values first (preferably a single value)

namespace Sudoku {
	class Program {

		static internal int[,] board = new int[9,9];
		internal static bool[,] original = new bool[9, 9];
		internal static bool solved = false;
		static internal int nodeCount = 0;

		static void Main(string[] args) {
			stringToBoard();
			printBoard();
			solve();
			Console.WriteLine("Solved! Node count: " + nodeCount);
		}

		// returns true of board is full; false otherwise
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

		// Generates raw moves
		static List<Move> rawMoveGenerator() {
			List<Move> rawMoveList = new List<Move>();

			// Loops over all squares
			for (int i = 0; i < 9; i++) {
				for (int j = 0; j < 9; j++) {

					// If square is empty
					if (board[i, j] == 0){
						
						//Keeps track of frequency of numbers (1-9) in peer squares
						int[] numFrequency = new int[10];

						// Loops through all peer squares and increments the frequency array
						// Loops through rows
						for (int k = 0; k < 9; k++){ 
							numFrequency[board[i, k]] ++;
						}
						// Loops through columns
						for (int l = 0; l < 9; l++){
							numFrequency[board[l, j]] ++;
						}
						// Loops through blocks (only elements that weren't included in the row or column loop)
						for (int m = 0; m < 3; m++){
							for (int n = 0; n < 3; n++){
								if (i/3*3 + m != i && j/3*3 + n != j) {
									numFrequency[board[i / 3 * 3 + m, j / 3 * 3 + n]]++;
									
								}
							}
						}
						// Calculates number of possible values that can be put into the square (number of elements in the frequency array whose value is 0)
						int optionsForSquare = 0;
						for (int o = 1; o < 10; o++){
							if (numFrequency[o] == 0) optionsForSquare ++;
						}
						// If number of possible numbers for empty square is 0, then we have a contradiction: return a null list
						// Otherwise, add all of the possible values for that empty square to the move list (with a higher score for empty squares with fewer possible values)
						if (optionsForSquare == 0) {
							return null;
						} else {
							for (int p = 1; p < 10; p++) {
								int frequency = numFrequency[p];
								Debug.Assert(frequency >= 0 && frequency <= 3);
								if (frequency == 0) {
									rawMoveList.Add(new Move(p, i, j, 11 - optionsForSquare));
								}
							}
						}
					}
				}
			}
			return rawMoveList;
		}

		static List<Move> refinedMoveGenerator() {
			List<Move> refinedMoveList = rawMoveGenerator();

			if (refinedMoveList == null){
				return null;
			}

			for (int i = 0; i < 9; i++) {
				int[] rowFrequency = new int[10];
				for (int j = 0; j < 9; j++) {
					foreach (Move move in refinedMoveList) {
						if (move.row == i && move.column == j) {
							rowFrequency[move.number]++;
						}
					}
				}
				for (int k = 1; k < 10; k++) {
					if (rowFrequency[k] == 1) {
						foreach (Move move in refinedMoveList.ToList()) {
							if (move.row == i && move.number == k) {
								//Console.WriteLine("Row:" + move);
								removeOtherMovesForSquare(move, refinedMoveList);
							}
						}
					}
				}	
			}

			for (int l = 0; l < 9; l++) {
				int[] columnFrequency = new int[10];
				for (int m = 0; m < 9; m++) {
					foreach (Move move in refinedMoveList) {
						if (move.row == m && move.column == l) {
							columnFrequency[move.number]++;
						}
					}
				}
				for (int n = 1; n < 10; n++) {
					if (columnFrequency[n] == 1) {
						foreach (Move move in refinedMoveList.ToList()) {
							if (move.column == l && move.number == n) {
								//Console.WriteLine("Column:" + move);
								removeOtherMovesForSquare(move, refinedMoveList);
							}
						}
					}
				}	
			}

			for (int o = 0; o < 9; o += 3) {
				for (int p = 0; p < 9; p += 3) {
					int[] blockFrequency = new int[10];
					for (int q = 0; q < 3; q++) {
						for (int r = 0; r < 3; r++) {
							foreach (Move move in refinedMoveList) {
								if (move.row == o + q && move.column == p + r) {
									blockFrequency[move.number]++;
								}
							}
						}
					}
					for (int s = 1; s < 10; s++) {
						if (blockFrequency[s] == 1) {
							foreach (Move move in refinedMoveList.ToList()) {
								if (move.row >= o && move.row < o + 3 && move.column >= p && move.column < p+3 && move.number == s) {
									//Console.WriteLine("Block:" + move);
									removeOtherMovesForSquare(move, refinedMoveList);
								}
							}
						}
					}
				}
			}		
			
			
			// Selection sort the move list
			for (int q=0; q < refinedMoveList.Count(); q++) {
				int bestScore = -100;
				int bestPosition = -1;
				for (int r = q; r < refinedMoveList.Count(); r++) {
					if (refinedMoveList[r].score > bestScore) {
						bestScore = refinedMoveList[r].score;
						bestPosition = r;
					}
				}
				Move temp = refinedMoveList[q];
				refinedMoveList[q] = refinedMoveList[bestPosition];
				refinedMoveList[bestPosition] = temp;
			}

			for (int i = refinedMoveList.Count() - 1; i >= 0; i--){
				if (refinedMoveList[i].score == -99){
					refinedMoveList.RemoveAt(i);
				}
			}
			if (unitValueContradiction()){
				return null;
			} else{
				return refinedMoveList;	
			}
		}

		private static void removeOtherMovesForSquare(Move move, List<Move> moveList) {
			int row = move.row;
			int column = move.column;
			int number = move.number;
			for (int i = 0; i < moveList.Count; i++) {
				if (moveList[i].row == row && moveList[i].column == column && moveList[i].number != number) {
					moveList[i] = new Move(moveList[i].number, row, column, -99);
				}
				if (moveList[i].row == row && moveList[i].column == column && moveList[i].number == number) {
					moveList[i] = new Move(number, row, column, 100);
				}
			}
		}

		// Returns true if there is a unit in the board that can't have a certain value
		private static bool unitValueContradiction () {
			
			return false;
		}

		// Solves the sudoku grid using recursive depth-first search
		static void solve() {
			if (isFull()){
				solved = true;
				printBoard();
				return;
			}
			List<Move> moveList = refinedMoveGenerator();

			// If the move list is null, that means there was a contradiction and board is illegal, so return immediately
			if (moveList == null) {
				return;
			}

			printBoard();
			Thread.Sleep(500);
			
			// If board is "legal", loop through all the moves
			foreach (Move move in moveList) {

				board[move.row, move.column] = move.number;
				solve();
				board[move.row, move.column] = 0;
				nodeCount++;

				// If the board is solved, back up to the root
				if (solved) {
					return;
				}
			}	
		}

		// Takes an input string and converts it to a 9x9 array
		static void stringToBoard() {
			Console.WriteLine("Enter 81-character sudoku string:");
			String boardString = Console.ReadLine();
			int counter = 0;
			for (int i = 0; i < 9; i++) {
				for (int j = 0; j < 9; j++) {
					board[i, j] = Convert.ToInt32(boardString[counter++]) - '0';
				}
			}
			for (int i = 0; i < 9; i++) {
				for (int j = 0; j < 9; j++) {
					if (board[i, j] != 0) {
						original[i, j] = true;
					}
				}
			}
		}

		// Prints the board
		static void printBoard() {
			Console.Clear();
			for (int i = 0; i < 10; i++) {
				Console.WriteLine(i != 0 ? i < 9 ? i % 3 == 0 ? "╠═══╪═══╪═══╬═══╪═══╪═══╬═══╪═══╪═══╣" : "╟───┼───┼───╫───┼───┼───╫───┼───┼───╢" : "╚═══╧═══╧═══╩═══╧═══╧═══╩═══╧═══╧═══╝" : "╔═══╤═══╤═══╦═══╤═══╤═══╦═══╤═══╤═══╗");
				for (int j = 0; j < 10; j++) {
					if (i == 9) {
						Console.Write("");
					} else {
						if (j != 0) {
							if (board[i, (j - 1)] != 0) {
								if (original[i, (j - 1)]) {
									Console.ForegroundColor = ConsoleColor.Red;
									Console.Write(board[i, (j - 1)].ToString());
									Console.ForegroundColor = ConsoleColor.White;
								} else {
									Console.Write(board[i, (j - 1)].ToString());
								}								
							} else {
								Console.Write(" ");
							}
						}
						if (j % 3 == 0 && j != 0) {
							Console.Write(" ║ ");
						} else if (j % 3 == 0 && j == 0) {
							Console.Write("║ ");
						} else {
							Console.Write(" │ ");
						}
						if (j == 9) {
							Console.Write("\n");
						} else {
							Console.Write("");
						}
					}
				}
			}
		}
	}

	// Move struct
	struct Move {
		internal int number, row, column, score;
		
		public Move(int number, int row, int column, int score = 0) {
			this.number = number;
			this.row = row;
			this.column = column;
			this.score = score;
		}

		public override string ToString() {
			string move = "(" + row + ", " + column + "): " + number + "   (" + score + ")";
			return move;
		}
	}
}
