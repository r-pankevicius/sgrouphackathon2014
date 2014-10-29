-------------------------------------------------------------------------
- Samagon.BigNumber version 1, Copyright (C) 2014 Samagon & CO          -
- Samagon.BigNumber comes with ABSOLUTELY NO WARRANTY; for details      -
- navigate to www.gnu.org/licenses/gpl-2.0.html. This is free software, -
- and you are welcome to redistribute it under certain conditions;      -
- navigate to www.gnu.org/licenses/gpl-2.0.html for details.            -
-------------------------------------------------------------------------

1. Extract Samagon.zip to selected directory on your computer.

2. Double-click NumberOfSamagon.sln, in order to open the solution in VS 2012.

3. Build the solution.

4. Run the program.

5. Program will accept arguments:

	-? or -h	CADIE will help you with the possible arguments.

	-i			CADIE will expect 2 operands, specified in 2 input files.
				File names shall be separated by the space. You can specify
				only one file name, then both operands will be read from the same file.

	-o			CADIE will expect file name where to put results.

	-a or -+	indicates addition as the operation between operands. This is default
				operation, if none is specified.

	-s or --	indicates subtraction as the operation between operands.

	-f			fast mode (if you know your input files are correct)

	-god		you can feel like God, since you will not be asked for annoying confirmations.