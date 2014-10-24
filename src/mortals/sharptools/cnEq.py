'''
CADIE numbers comparer, Melbourne 2013 version, see http://youtu.be/LOJbM0aXZp0?t=5s

Usage:
    cnEqSlow.py file1 file2

errorlevel:
    0 : file1 = file2
    1 : file1 > file2
    2 : file1 < file2
    other: total disaster, correct file(s) and resubmit the job.
'''

import sys
import os.path
import numpy
from CnEqCommon import *


# -----------
# CADIE Number File
class CnFile:
    
    MINUS = ord('-')
    PLUS  = ord('+')
    ZERO  = ord('0')
    #ONE   = ord('1')
    NINE  = ord('9')
    
    def __init__(self, fileName):
        self.fileName = fileName
        self.file = None
        self.length = 0
        self.currentPos = 0
        self.endReached = False
        self.sign = None # negative: -1, positive: 1
        #self.buffer = None
        self.isZero = None
    
    def __enter__(self):
        # Does the file exist?
        if not os.path.isfile(self.fileName):
            # HTTP 404 Not Found
            raise RanAndStuckError(404, 'I HAVE NO FILE AND I MUST SCREAM: %s' % self.fileName)
    
        # Empty file is invalid too
        self.length = os.path.getsize(self.fileName)
        if self.length == 0:
            # HTTP 204 No Content
            raise RanAndStuckError(204, 'THROW STICK BEFORE RETRIEVING: %s' % self.fileName)

        # Pray and open as memory-mapped array
        self.file = numpy.memmap(self.fileName, 'uint8', 'r')
        #print('1st CHAR: %c, LAST CHAR %c' % (self.file[0], self.file[self.length - 1])) 
        #self.file = open(self.fileName, 'rb')
        self._readTo1stSignificantDigit()
        return self
    
    @staticmethod
    def IsDigit(ch):
        return ch >= CnFile.ZERO and ch <= CnFile.NINE
    
    def _readByte(self):
        ch = self.file[self.currentPos]
        self.currentPos += 1
        return ch

    # Moves tape reader head to 1st digit or sets self.isZero = True
    def _readTo1stSignificantDigit(self):
        
        is1stSignificantNum = lambda digit: digit > CnFile.ZERO and digit <= CnFile.NINE
        
        # 1st byte always defines sign
        ch = self._readByte()
        if ch == CnFile.MINUS:
            self.sign = -1
        elif ch == CnFile.PLUS:
            self.sign = 1
        elif CnFile.IsDigit(ch):
            self.sign = 1
        else:
            self._raiseBadChar()
                
        if (ch == CnFile.MINUS or ch == CnFile.PLUS):
            ch = self._readByte()
                
        # Handle '0'-s at start of file
        while (ch == CnFile.ZERO) and (self.currentPos < self.length):
            ch = self._readByte()
            
        if (not is1stSignificantNum(ch)):
            if ch == CnFile.ZERO:
                self.isZero = True
                self.sign = 1 # Treat zero as positive zero
            else:
                self._raiseBadChar()
        else:        
            self.isZero = False
        
    def _raiseBadChar(self):
        # 406 Not Acceptable
        raise RanAndStuckError(406, 'THAT\'S TOO COMPLEX FOR ME TO GRASP: \'%c\' at pos %d in file "%s"' % \
                                       (self.file[self.currentPos], self.currentPos, self.fileName))

    def __exit__(self, aType, value, t):
        if self.file <> None:
            #self.file.close()
            pass # TODO : how to close/detach memmap from file?

    def scanToTheEndOfNumber(self):
        pass

# -----------
def main(argv):
    
    if len(argv) <> 2:
        usage()
        sys.exit(400) # HTTP 400 Bad Request
        
    fileName0 = argv[0]
    fileName1 = argv[1]

    try:
        with CnFile(fileName0) as cn0:
            with CnFile(fileName1) as cn1:
            #    result = compareCadieNumbers0(cn0, cn1)
                # Almost here...
                cn0.scanToTheEndOfNumber()
                cn1.scanToTheEndOfNumber()
                # Print file1<file2 or so
            #    print('%s %s %s' % (fileName0, toCompareSign(result), fileName1))
            #    sys.exit(toExitCode(result))
    except RanAndStuckError as expectedError:
        print('Error %d: %s' % (expectedError.errorCode, expectedError.message))
        sys.exit(expectedError.errorCode)
    except RuntimeError:
        # HTTP 450 Blocked by Windows Parental Controls
        print('Error 450: PROGRAM FELL OFF THE EDGE ', sys.exc_info()[0], sys.exc_info()[1])
        sys.exit(450)


# -----------
def usage():
    print('CADIE numbers comparer.')
    print('Usage:')
    print('  %s file1 file2' % sys.argv[0])

# -----------
if __name__ == '__main__':
    main(sys.argv[1:])