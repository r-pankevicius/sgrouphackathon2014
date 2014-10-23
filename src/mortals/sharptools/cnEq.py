'''
CADIE numbers comparer.

Usage:
    cnEq.py file1 file2

errorlevel:
    0 : file1 = file2
    1 : file1 > file2
    2 : file1 < file2
    other: total disaster, correct file(s) and resubmit the job.
'''

import sys
import os.path
#from curses.ascii import isdigit

# -----------
# Runtime exception with error code
class RanAndStuckError(RuntimeError):
    
    def __init__(self, errorCode, message):
        RuntimeError.__init__(self, message)
        self.errorCode = errorCode

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
    
    def __enter__(self):
        # Does the file exist?
        if not os.path.isfile(self.fileName):
            # HTTP 404 Not Found
            raise RanAndStuckError(404, 'File "%s" doesn\'t exist.' % self.fileName)
    
        # Empty file is invalid too
        self.length = os.path.getsize(self.fileName)
        if self.length == 0:
            # HTTP 204 No Content
            raise RanAndStuckError(204, 'File "%s" is empty.' % self.fileName)

        # Pray and open
        self.file = open(self.fileName, 'rb')
        self._readTo1stSignificantDigit()
        return self
    
    def __exit__(self, aType, value, t):
        if self.file <> None:
            self.file.close()
    
    # @returns negative: -1, positive: 1
    def _readTo1stSignificantDigit(self):
        
        isdigit = lambda digit: digit >= '0' and digit <= '9'
        is1stSignificantNum = lambda digit: digit >= '1' and digit <= '9'
        
        # 1st byte always defines sign
        self.buffer = self.file.read(1)
        self.currentPos += 1
        if self.sign == None:
            if self.buffer == '-':
                self.sign = -1
            elif self.buffer == '+':
                self.sign = 1
            elif isdigit(self.buffer):
                self.sign = 1
            else:
                self.raiseBadChar()
                
        if (self.buffer in ['-', '+']):
            self.buffer = self.file.read(1)
            self.currentPos += 1
                
        # Handle '0'-s at start of file
        while (self.buffer == '0') and (self.currentPos < self.length):
            self.buffer = self.file.read(1)
            self.currentPos += 1
            
        if (not is1stSignificantNum(self.buffer)) and (not self._isZero()):
            self._raiseBadChar()
        
        
        #TODO: _isZero - bad method, can be CR,LF...
        #if self.buffer in ['-', '+']:
        #    return self.sign
        
    def _raiseBadChar(self):
        # 406 Not Acceptable
        raise RanAndStuckError(406, 'Bad char \'%c\' at pos %d in file "%s"' % \
                                       (self.buffer, self.currentPos, self.fileName))

    def _isZero(self):
        return (self.buffer == '0' and self.currentPos == self.length)

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
                result = compareCadieNumbers(cn0, cn1)
                # Print file1<file2 or so
                print('%s %s %s' % (fileName0, toCompareSign(result), fileName1))
                sys.exit(toExitCode(result))
    except RanAndStuckError as expectedError:
        print('Error %d: %s' % (expectedError.errorCode, expectedError.message))
        sys.exit(expectedError.errorCode)
    except RuntimeError:
        # HTTP 450 Blocked by Windows Parental Controls
        print('Error 450: OMG!!! Unexpected error ', sys.exc_info()[0], sys.exc_info()[1])
        sys.exit(450)

# -----------
def toCompareSign(resultNumber):
    if resultNumber == 0:
        return '='
    elif resultNumber > 0:
        return '>'
    else:
        return '<'

# -----------
def toExitCode(resultNumber):
    if resultNumber == 0:
        return 0
    elif resultNumber > 0:
        return 1
    else:
        return 2

# -----------
def compareCadieNumbers(cn0, cn1):
    
    if cn0.sign <> cn1.sign:
        return cn0.sign - cn1.sign

    #TODO: type something more here
    
    return 0


# -----------
"""
Reads a sign::=oneof(-1,+1) and first digit::=number[0..9] in file f.
Returns [sign, first digit]
"""
def readSignAndFirstDigit(f):
    byte = f.read(1)
    if byte == '-':
        return -1
    elif byte == '+':
        return 1
    elif byte >= '0' and byte <= '9' :
        return 1
    else:
        raise RuntimeError('BAD, BAD %s' % f.name)
        #return [byte == '-' ? -1 : +1, None]
    #TODO: type something more here

# -----------
def assureFileExistsAndNotEmpty(fileName, fileIdx):

    if not os.path.isfile(fileName):
        print('File "%s" doesn\'t exist.' % fileName)
        sys.exit(404 + fileIdx) # HTTP 404 Not Found, 405 Method Not Allowed

    # Empty file is invalid
    fileSize = os.path.getsize(fileName)
    if fileSize == 0:
        print('File "%s" is empty.' % fileName)
        sys.exit(204 + fileIdx) # HTTP 204 No Content, 205 Reset Content


# -----------
def usage():
    print('CADIE numbers comparer.')
    print('Usage:')
    print('  %s file1 file2' % sys.argv[0])

# -----------
if __name__ == '__main__':
    main(sys.argv[1:])
