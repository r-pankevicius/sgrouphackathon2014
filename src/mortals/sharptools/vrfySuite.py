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

import sys, subprocess, datetime
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

    def __init__(self, teamName, pathToExe, formatArgsCallback):
        '''
        Constructor
        '''
        self.teamName = teamName
        self.pathToExe = pathToExe
        self.formatArgsCallback = formatArgsCallback
        
    def runSuite(self):
        self.hr()
        self.info('PLEASE NOTE: THIS IS A MOMENT OF TRUTH FOR %s' % self.teamName)
        self.info('TEAM=%s' % self.teamName)
        self.info('PROGRAM=%s' % self.pathToExe)
        self.hr()
        
        #TODO: add all tests
        #self.run('f1', '+', 'f2', 'r')
        
        self.hr()
        
    def run(self, f1, op, f2, result):
        args = self.formatArgsCallback(f1, op, f2, result)
        commandLine = '%s %s' % (self.pathToExe, args)
        self.info('EXEC %s' % commandLine)
        startTime = datetime.datetime.now()
        try:
            retcode = subprocess.call(commandLine, shell=False)
            if retcode < 0:
                self.info('ERROR: terminated by signal %d' % -retcode)
            else:
                self.info('RETCODE %d' % -retcode)
        except OSError as e:
            self.info('ERROR: execution failed: %s' % str(e))
        endTime = datetime.datetime.now()
        miliseconds = (endTime - startTime).microseconds / 1000
        self.info('TOOK miliseconds: %d' % miliseconds)
            
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
    
    if not (teamSectionName in config.sections()):
        print('No section %s in %s' % (teamSectionName, iniFileName))
        return
    
    teamName = config.get(teamSectionName, 'Name')
    pathToProgram = config.get(teamSectionName, 'Program')
    args = config.get(teamSectionName, 'Args')
    #TODO: How to define args format?
    
    suite = VerifySuite(teamName, pathToProgram, \
                        lambda f1, op, f2, result : '%s %s %s %s' % (f1, op, f2, result) \
                        )
    suite.runSuite()
    
# -----------
if __name__ == '__main__':
    main(sys.argv[1:])