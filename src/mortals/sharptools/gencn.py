'''
CADIE numbers generator.

Sample usage to generate CADIE number with requested length:
    gencn.py 101    => 101 byte
    gencn.py 12K    => 12 kilobytes
    gencn.py 200M   => 200 megs
    gencn.py 10G    => 10 gigs

Generator uses Champernowne constant
(@see http://en.wikipedia.org/wiki/Champernowne_constant)
but adds some "noise" in between iterations.    
'''

if __name__ == '__main__':
    pass