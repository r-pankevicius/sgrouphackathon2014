'''
CADIE numbers comparer, V1

Usage:
    cnEqSlow.py file1 file2

errorlevel:
    0 : file1 = file2
    1 : file1 > file2
    2 : file1 < file2
    other: total disaster, correct file(s) and resubmit the job.
'''

import os.path
import numpy
from CnEqCommon import *

# -----------
# CADIE Number File
class CnFile:
    
    def __init__(self, fileName):
        self.fileName = fileName
        self.file = None
        self.length = 0
        self.currentPos = 0
        self.endReached = False
        self.sign = None # negative: -1, positive: 1
        self.buffer = None
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

        # Pray and open
        self.file = open(self.fileName, 'rb')
        self._readTo1stSignificantDigit()
        return self
    
    def __exit__(self, aType, value, t):
        if self.file <> None:
            self.file.close()

    @staticmethod
    def IsDigit(ch):
        return ch >= '0' and ch <= '9'
    
    # Moves tape reader head to 1st digit or sets self.isZero = True
    def _readTo1stSignificantDigit(self):
        
        is1stSignificantNum = lambda digit: digit >= '1' and digit <= '9'
        
        # 1st byte always defines sign
        self.buffer = self.file.read(1)
        self.currentPos += 1
        if self.buffer == '-':
            self.sign = -1
        elif self.buffer == '+':
            self.sign = 1
        elif CnFile.IsDigit(self.buffer):
            self.sign = 1
        else:
            self._raiseBadChar()
                
        if (self.buffer in ['-', '+']):
            self.buffer = self.file.read(1)
            self.currentPos += 1
                
        # Handle '0'-s at start of file
        while (self.buffer == '0') and (self.currentPos < self.length):
            self.buffer = self.file.read(1)
            self.currentPos += 1
            
        if (not is1stSignificantNum(self.buffer)):
            if self.buffer == '0':
                self.isZero = True
                self.sign = 1 # Treat zero as positive zero
            else:
                self._raiseBadChar()
        else:        
            self.isZero = False
        
    def _raiseBadChar(self):
        # 406 Not Acceptable
        raise RanAndStuckError(406, 'THAT\'S TOO COMPLEX FOR ME TO GRASP: \'%c\' at pos %d in file "%s"' % \
                                       (self.buffer, self.currentPos, self.fileName))
        
    def nextDigit(self):
        digit = self.buffer
        if digit == None:
            # 509 Bandwidth Limit Exceeded
            raise RanAndStuckError(509, 'I\'VE FORGOTTEN WHAT I WAS ABOUT TO SAY: at pos %d in file "%s"' % \
                                           (self.currentPos, self.fileName))
            
        if self.currentPos < self.length:
            self.buffer = self.file.read(1)
            self.currentPos += 1
            if not CnFile.IsDigit(self.buffer):
                if ord(self.buffer) in [10, 13]:
                    self.endReached = True
                else:
                    self._raiseBadChar()
        else:
            self.endReached = True
            self.buffer = None
        
        return digit
    
    def hasMoreDigits(self):
        return CnFile.IsDigit(self.buffer)
    
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
    digitsDiff = lambda x, y : (x - '0') - (y - '0')
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