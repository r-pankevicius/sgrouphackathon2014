'''
Verification tests suite.
For each team contains:
    references to program executables,
    instructions how to build command line.
    
For each verify corectness suite contains:
For each verify speed suite contains:
    references to pair of files and operator (+ || -)
    reference to correct result file.
'''

import os, shutil, sys, subprocess, datetime
import ConfigParser

class VerifySuite:
    '''
    Tests:
    
    16k\a + 16k\b => r1
    r1 == a6k\c
    16k\c - 16k\b => r2
    r2 == 16k\a
    16k\b - 16k\c => r3
    r3 == 16k\-a
    16k\-b2 + 16k\-a2 => r4
    r4 == 16k\-c
    16k\-b2 - 16k\a2 => r5
    r5 == 16k\-c
    
    16k\a3 + 1k\a3 => r6
    r6 - 1k\c3 => r7
    r7 + 1k\b3 => r8
    r8 == 16k\a3
    
    1k\a - 16k\a => r9
    r9 - 16k\b => r10
    r10 + 1k\b => r11
    r11 - 1k\c => r12
    r12 == 16k\-c
    
    10g\a + 10g\b => r100
    r100 - 512m\a => r101
    r101 - 10g\b => r102
    r102 + 512m\a => r103
    r103 == 10g\a
    '''

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
        
    def verifyEqual(self, f1, f2):
        self.info('ASSERT %s = %s' % (f1, f2))
        retcode = os.system('cnEq.py %s %s' % (f1, f2))
        if retcode == 0:
            return 0
        self.info('ASSERT FAILED!')
        return 1

    """ Teams are not ready for it yet...
    def runSuite(self):
        self.failuresCount = 0
        self.cleanOutputDir()
        self.hr()
        self.info('PLEASE NOTE: THIS IS A MOMENT OF TRUTH FOR %s' % self.teamName)
        self.info('TEAM=%s' % self.teamName)
        self.info('PROGRAM=%s' % self.pathToExe)
        self.hr()
        
        #TODO: add all tests
        self.run(self.zeroFile, '+', self.zeroFile, self.testDataOutputFolder + 'r0.txt')
        
        self.hr()
    """
        
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
    
    if len(argv) <> 1:
        print('Pass team as argument (see section in %s)' % iniFileName)
        return
    
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
    #suite.runSuite()
    suite.runSmokeTests()
    print('TOTAL NUMBER OF FAILURES: %d' % suite.failuresCount)
    
# -----------
if __name__ == '__main__':
    main(sys.argv[1:])