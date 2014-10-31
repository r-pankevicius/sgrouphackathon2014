'''
CADIE numbers comparer, V2

Usage:
    cnEq.py file1 file2

errorlevel:
    0 : file1 = file2
    1 : file1 > file2
    2 : file1 < file2
    other: total disaster, correct file(s) and resubmit the job.
'''

import os.path
import numpy
import sys
from CnEqCommon import *

# -----------
# CADIE Number File
class CnFile:
    
    MINUS = ord('-')
    PLUS  = ord('+')
    ZERO  = ord('0')
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
        self.endpos = self.length - 1

        # Pray and open as memory-mapped array
        self.file = numpy.memmap(self.fileName, 'uint8', 'r')
        #print('1st CHAR: %c, LAST CHAR %c' % (self.file[0], self.file[self.length - 1])) 
        #self.file = open(self.fileName, 'rb')
        self._readTo1stSignificantDigit()
        return self
    
    @staticmethod
    def IsDigit(ch):
        return ch >= CnFile.ZERO and ch <= CnFile.NINE
    
    # Moves tape reader head to 1st digit or sets self.isZero = True
    def _readTo1stSignificantDigit(self):
        
        is1stSignificantNum = lambda digit: digit > CnFile.ZERO and digit <= CnFile.NINE
        
        # 1st byte always defines sign
        ch = self.file[0]
        if ch == CnFile.MINUS:
            self.sign = -1
            self.currentPos = 1
        elif ch == CnFile.PLUS:
            self.sign = 1
            self.currentPos = 1
        elif CnFile.IsDigit(ch):
            self.sign = 1
            self.currentPos = 0
        else:
            self._raiseBadChar()
                
        if (ch == CnFile.MINUS or ch == CnFile.PLUS):
            if self.length == 1:
                self._raiseBadChar()
            ch = self.file[1]
                
        # Handle '0'-s at start of file
        while (ch == CnFile.ZERO) and (self.currentPos != self.endpos):
            self.currentPos += 1; ch = self.file[self.currentPos]
            
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

    def nextDigit(self):
        if self.currentPos == self.endpos:
            self.endReached = True
        
        ch = self.file[self.currentPos]; self.currentPos += 1
        if not CnFile.IsDigit(ch):
            if ch in [10, 13]:
                self.endReached = True
            else:
                self.currentPos -= 1
                self._raiseBadChar()
        
            
        return ch
    
    def hasMoreDigits(self):
        return not self.endReached and CnFile.IsDigit(self.file[self.currentPos])
    
    def scanToTheEndOfNumber(self):
        while not self.endReached:
            self.nextDigit()


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
                result = compareCadieNumbers0(cn0, cn1)
                # Almost here...
                cn0.scanToTheEndOfNumber()
                cn1.scanToTheEndOfNumber()
                # Print file1<file2 or so
                print('%s %s %s' % (fileName0, toCompareSign(result), fileName1))
                sys.exit(toExitCode(result))
    except RanAndStuckError as expectedError:
        print('Error %d: %s' % (expectedError.errorCode, expectedError.message))
        sys.exit(expectedError.errorCode)
    except RuntimeError:
        # HTTP 450 Blocked by Windows Parental Controls
        print('Error 450: PROGRAM FELL OFF THE EDGE ', sys.exc_info()[0], sys.exc_info()[1])
        sys.exit(450)

# -----------
# @returns sign (cn0 - cn1) as soon as it suspects it can guess result...
def compareCadieNumbers0(cn0, cn1):
    
    if cn0.sign <> cn1.sign:
        return cn0.sign - cn1.sign

    if cn0.isZero:
        if cn1.isZero:
            return 0
        else:
            return -cn1.sign
    elif cn1.isZero:
        return cn0.sign
    
    assertEqual(cn0.sign, cn1.sign)
    theSign = cn0.sign
    
    # Need to read CADIE numbers in parallel and compare digits.
    digitsDiff = lambda x, y : (x - CnFile.ZERO) - (y - CnFile.ZERO)
    resultOnDigitsDiff = lambda x, y : theSign * digitsDiff(x, y) / numpy.sign(digitsDiff(x, y)) 
    
    while True:
        if cn0.hasMoreDigits():
            if cn1.hasMoreDigits():
                d0 = cn0.nextDigit()
                d1 = cn1.nextDigit()
                if d0 == d1:
                    continue
                else:
                    return resultOnDigitsDiff(d0, d1)
            else:
                return theSign
        else:
            return -theSign if cn1.hasMoreDigits() else 0
    
    iWishIWasntHere()

# -----------
if __name__ == '__main__':
    main(sys.argv[1:])