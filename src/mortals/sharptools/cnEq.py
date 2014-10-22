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


# -----------
class CnFile:
    def __init__(self, aFileOpened_rb):
        self.file = aFileOpened_rb
        self.length = os.path.getsize(aFileOpened_rb.name)
        self.currentPos = 0
        self.endReached = False
        self.sign = None

# -----------
def main(argv):

    if len(argv) <> 2:
        usage()
        sys.exit(400) # HTTP 400 Bad Request

    fileName0 = argv[0]
    assureFileExistsAndNotEmpty(fileName0, 0)

    fileName1 = argv[1]
    assureFileExistsAndNotEmpty(fileName1, 1)

    with open(fileName0, 'rb') as f0:
        with open(fileName1, 'rb') as f1:
            try:
                result = compareFiles(f0, f1)
                # Print file1<file2 or so
                print('%s %s %s' % (fileName0, toCompareSign(result), fileName1))
                sys.exit(toExitCode(result))
            except RuntimeError as re:
                print('Oops! ' + re.strerror)
                sys.exit(450) # HTTP 450 Blocked by Windows Parental Controls
                
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
def compareFiles(f0, f1):
    
    cn0 = CnFile(f0)
    cn1 = CnFile(f1) 

    sgn0 = readSignAndFirstDigit(f0)
    sgn1 = readSignAndFirstDigit(f1)

    #TODO: type something more here
    
    return sgn0 - sgn1


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
