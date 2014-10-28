'''
Verification tests suite.
Configuration: .ini file
Usage:
Run smoke tests for team1:
    vrfySuite.py team1
Run real tests for team1:
    vrfySuite.py team1 go!
'''

import os, sys, subprocess, datetime
import ConfigParser

class VerifySuite:

    Prefix = '@#$$$$: '

    def __init__(self, testDataFolder, teamName, pathToExe, argsFormat):
        '''
        Constructor
        '''
        self.teamName = teamName
        self.pathToExe = pathToExe
        self.argsFormat = argsFormat
        
        self.testDataRootFolder = testDataFolder + '4test\\'
        self.testDataOutputFolder = testDataFolder + 'output\\'
        
        # Sandbox folder for smoke tests
        self.smokeTestDataRootFolder = '..\\..\\..\\data\sandbox\\'
        
        self.failuresCount = 0
        self.badResultCount = 0
        self.runMonsterTests = False
        
    def formatArgs(self, file1, operation, file2, resultFile):
        return self.argsFormat.format(file1, operation, file2, resultFile) 
        
    def smokeTestFile(self, fileName):
        return self.smokeTestDataRootFolder + fileName  

    def dataFile(self, fileName):
        return self.testDataRootFolder + fileName  

    def resultFile(self, fileName):
        return self.testDataOutputFolder + fileName
    
    def cleanOutputDir(self):
        self.info('Cleaning %s' % self.testDataOutputFolder)
        for name in os.listdir(self.testDataOutputFolder):
            fullpath = os.path.join(self.testDataOutputFolder, name)
            os.remove(fullpath)
                
    def runSmokeTests(self):
        self.failuresCount = 0
        self.cleanOutputDir()
        self.hr()
        self.info('PLEASE NOTE: THIS IS A MOMENT OF TRUTH FOR %s' % self.teamName)
        self.info('TEAM=%s' % self.teamName)
        self.info('PROGRAM=%s' % self.pathToExe)
        self.hr()
        
        # TEST 1: 0 + 0 = 0
        testNo = 1
        self.runSmokeTest(testNo, '0.txt', '+', '0.txt', '0.txt'); testNo += 1
        # etc.
        self.runSmokeTest(testNo, '0.txt', '-', '0.txt', '0.txt'); testNo += 1
        self.runSmokeTest(testNo, '0.txt', '+', '1.txt', '1.txt'); testNo += 1
        self.runSmokeTest(testNo, '0.txt', '-', '1.txt', '-1.txt'); testNo += 1
        self.runSmokeTest(testNo, '1.txt', '+', '0.txt', '1.txt'); testNo += 1
        self.runSmokeTest(testNo, '1.txt', '-', '0.txt', '1.txt'); testNo += 1
        self.runSmokeTest(testNo, '0.txt', '+', '-1.txt', '-1.txt'); testNo += 1
        self.runSmokeTest(testNo, '0.txt', '-', '-1.txt', '1.txt'); testNo += 1
        self.runSmokeTest(testNo, '-1.txt', '+', '1.txt', '0.txt'); testNo += 1
        self.runSmokeTest(testNo, '-1.txt', '+', '0.txt', '-1.txt'); testNo += 1
        
        self.hr()
        
    def runSmokeTest(self, testNo, f1, op, f2, expected):
        n1 = self.smokeTestFile(f1)
        n2 = self.smokeTestFile(f2)
        result = self.resultFile('r%d.txt' % testNo)
        seemsOk = self.run(n1, op, n2, result)
        if not seemsOk:
            self.failuresCount += 1
        else:
            compareTo = self.smokeTestFile(expected)
            self.failuresCount += self.verifyEqual(result, compareTo)
        
    def doSuiteOp(self, f1, op, f2, result):
        self.info('PLEASE DO %s %s %s => %s' % (f1, op, f2, result))
        n1 = self.dataFile(f1)
        n2 = self.dataFile(f2)
        result = self.resultFile(result)
        seemsOk = self.run(n1, op, n2, result)
        if not seemsOk:
            self.failuresCount += 1
            return False
        return True
    
    def doSuiteOpR(self, r1, op, f2, result):
        self.info('PLEASE DO %s %s %s => %s' % (r1, op, f2, result))
        n1 = self.resultFile(r1)
        n2 = self.dataFile(f2)
        result = self.resultFile(result)
        seemsOk = self.run(n1, op, n2, result)
        if not seemsOk:
            self.failuresCount += 1
            return False
        return True
    
    def verifyEqual(self, f1, f2):
        self.info('ASSERT %s = %s' % (f1, f2))
        retcode = os.system('cnEq.py %s %s' % (f1, f2))
        if retcode == 0:
            return 0
        self.info('ASSERT FAILED!')
        return 1

    def assertSuiteResultEq(self, result, compareTo):
        r = self.resultFile(result)
        cmpTo = self.dataFile(compareTo)
        self.info('EXPECTING %s = %s' % (r, cmpTo))
        retcode = os.system('cnEq.py %s %s' % (r, cmpTo))
        if retcode == 0:
            return True
        self.info('EXPECTATION FAILED!')
        self.badResultCount += 1
        return False

    def runSuite(self):
        self.failuresCount = 0
        self.badResultCount = 0
        
        self.cleanOutputDir()
        
        self.hr()
        self.info('PLEASE NOTE: THIS IS A MOMENT OF TRUTH FOR %s' % self.teamName)
        self.info('TEAM=%s' % self.teamName)
        self.info('PROGRAM=%s' % self.pathToExe)
        self.hr()
        
        # 16k\a + 16k\b => r1
        self.doSuiteOp('16k\\a', '+', '16k\\b', 'r1')
        # r1 == 16k\c
        self.assertSuiteResultEq('r1', '16k\\c')
        
        # 16k\c - 16k\b => r2
        self.doSuiteOp('16k\\c', '-', '16k\\b', 'r2')
        # r2 == 16k\a
        self.assertSuiteResultEq('r2', '16k\\a')
        
        # 16k\b - 16k\c => r3
        self.doSuiteOp('16k\\b', '-', '16k\\c', 'r3')
        # r3 == 16k\-a
        self.assertSuiteResultEq('r3', '16k\\-a')
        
        # 16k\-b2 + 16k\-a2 => r4
        self.doSuiteOp('16k\\-b2', '+', '16k\\-a2', 'r4')
        # r4 == 16k\-c
        self.assertSuiteResultEq('r4', '16k\\-c')
        
        # 16k\-b2 - 16k\a2 => r5
        self.doSuiteOp('16k\\-b2', '-', '16k\\a2', 'r5')
        # r5 == 16k\-c
        self.assertSuiteResultEq('r5', '16k\\-c')
        
        # 16k\a3 + 1k\a3 => r6
        self.doSuiteOp('16k\\a3', '+', '1k\\a3', 'r6')
        # r6 - 1k\c3 => r7
        self.doSuiteOpR('r6', '-', '1k\\c3', 'r7')
        # r7 + 1k\b3 => r8
        self.doSuiteOpR('r7', '+', '1k\\b3', 'r8')
        # r8 == 16k\a3
        self.assertSuiteResultEq('r8', '16k\\a3')

        # 1k\a - 16k\a => r9
        self.doSuiteOp('1k\\a', '-', '16k\\a', 'r9')
        # r9 - 16k\b => r10
        self.doSuiteOpR('r9', '-', '16k\\b', 'r10')
        # r10 + 1k\b => r11
        self.doSuiteOpR('r10', '+', '1k\\b', 'r11')
        # r11 - 1k\c => r12
        self.doSuiteOpR('r11', '-', '1k\\c', 'r12')
        # r12 == 16k\-c
        self.assertSuiteResultEq('r12', '16k\\-c')
        
        
        if self.runMonsterTests:
            self.iAmMonster()

        self.hr()
        
    def iAmMonster(self):
        self.cleanOutputDir()
        
        # 10g\a + 10g\b => r100
        self.doSuiteOp('10g\\a', '+', '10g\\b', 'r100')
        # r100 - 512m\a => r101
        self.doSuiteOpR('r100', '-', '512m\\a', 'r101')
        # r101 - 10g\b => r102
        self.doSuiteOpR('r101', '-', '10g\\b', 'r102')
        # r102 + 512m\a => r103
        self.doSuiteOpR('r102', '+', '512m\\a', 'r103')
        # r103 == 10g\a
        self.assertSuiteResultEq('r103', '10g\\a')
        
    def run(self, f1, op, f2, result):
        args = self.formatArgs(f1, op, f2, result)
        commandLine = '%s %s' % (self.pathToExe, args)
        self.info('EXEC %s' % commandLine)
        startTime = datetime.datetime.now()
        success = True
        try:
            retcode = subprocess.call(commandLine, shell=False)
            if retcode < 0:
                self.info('ERROR: terminated by signal %d' % -retcode)
                success = False
            else:
                self.info('RETCODE %d' % -retcode)
        except OSError as e:
            self.info('ERROR: execution failed: %s' % str(e))
            success = False
        endTime = datetime.datetime.now()
        miliseconds = (endTime - startTime).microseconds / 1000
        self.info('TOOK miliseconds: %d' % miliseconds)
        return success
            
    def info(self, string):
        print('%s%s' % (VerifySuite.Prefix, string))
        
    def hr(self):
        self.info('**********************************************************************')
            
# -----------
def main(argv):
    
    iniFileName = 'vrfySuite.ini'
    
    if not len(argv) in [1, 2]:
        print('Pass team as argument (see section in %s).' % iniFileName)
        print('Program runs smoke tests by default.')
        print('An optional second argument is go! (run smaller performance tests) or all-in (pray before using it).')
        return
    
    runRealTests = False
    runMonsterTests = False
    if len(argv) == 2:
        if argv[1] == 'go!':
            runRealTests = True
        elif argv[1] == 'all-in':
            runRealTests = True
            runMonsterTests = True
    
    teamSectionName = argv[0]
    
    config = ConfigParser.ConfigParser()
    config.read(iniFileName)
    
    testDataFolder = config.get('Global', 'TestDataFolder')
    
    if not (teamSectionName in config.sections()):
        print('No section %s in %s' % (teamSectionName, iniFileName))
        return
    
    teamName = config.get(teamSectionName, 'Name')
    pathToProgram = config.get(teamSectionName, 'Program')
    argsFormat = config.get(teamSectionName, 'Args')
    
    suite = VerifySuite(testDataFolder, teamName, pathToProgram, argsFormat)
    
    if not runRealTests:
        suite.runSmokeTests()
    else:
        suite.runMonsterTests = runMonsterTests
        suite.runSuite()
        print('TOTAL NUMBER OF WRONG RESULTS: %d' % suite.badResultCount)
        
    print('TOTAL NUMBER OF FAILURES: %d' % suite.failuresCount)
    
# -----------
if __name__ == '__main__':
    main(sys.argv[1:])