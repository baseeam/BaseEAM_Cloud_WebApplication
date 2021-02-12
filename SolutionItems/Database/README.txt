Step1: Change config
PASSWORD=20101993
HOST=localhost
USER=root
DATABASE=BaseEAM
DB_FILE=dump.sql
EXCLUDED_TABLES=(
	UnitOfMeasure
	#User
)
Note: - # like comment code. Example use # to keep data of that table
Step 2: Rum cmd with adminstrator
Step 3: go and run .sh file
Step 4: After running done, open dump file to rename new db name.
Replace : Database: BaseEAM => Database: newBaseEAM. Use notepad++ and ctr + H
Step 5: Create a newBaseEAM in mysql => import data and structure from dump file
Step 6: Verify again