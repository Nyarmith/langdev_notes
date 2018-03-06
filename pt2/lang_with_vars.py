#!/usr/bin/env python3
import re
import sys
import math

regex_thing=r"\s+|;"

class ScratchLexer():
    def __init__(self, text):
        self.next  = 0
        self.words = re.compile(r"\s+").split(text)
        #self.words = []

    def nextWord(self):
        if self.next >= len(self.words):
            return None
        else:
            ret = self.words[self.next]
            self.next += 1
            return ret

def PRINT(scratch):
    print(scratch.stack.pop())

def PSTACK(scratch):
    print(scratch.stack)

#unary operator
def UNOP(scratch,f):
    if (len(scratch.stack) < 1):
        raise Exception("Not enough items on stack")
    scratch.stack.append(f(scratch.stack.pop()))

#binary operator
def BINOP(scratch, f):
    if (len(scratch.stack)<2):
        raise Exception("Not enough items on stack")
    t = scratch.stack.pop()
    scratch.stack.append(f(t, scratch.stack.pop))


def ADD(scratch):
    BINOP(scratch, lambda x,y:x+y)

def SUB(scratch):
    BINOP(scratch, lambda x,y:x-y)

def MULT(scratch):
    BINOP(scratch, lambda x,y:x*y)

def DIV(scratch):
    BINOP(scratch, lambda x,y:x/y)

def SQRT(scratch):
    UNOP(scratch, lambda x:math.sqrt(x))

def makeVariable(scratch):
    me = {value:0}
    return lambda: scratch.push(me)

def MKVAR(scratch):
    var_name = scratch.lexer.nextWord()
    if (var_name == None):
        raise Exception("Unexpected end of Input")
    scratch.define(var_name,makeVariable(scratch))


class Scratch():
    def __init__(self):
        self.dictionary={
                'PRINT' : PRINT,
                'PSTACK': PSTACK,
                '+' : ADD,
                '-' : SUB,
                '/' : DIV,
                '*' : MULT,
                'SQRT' : SQRT,
                'VAR' : MKVAR}
        self.stack = []
        self.lexer = None #Make lexer a member because w/ VAR stmt we have to look ahead

    def addWords(self, d):
        for word in d:
            self.dictionary[word.upper()] = d[word]
    #make a var
    def define(self,word,code):
        self.dictionary[word.upper()] = code
    
    def run(self, text):
        self.lexer   = ScratchLexer(text)
        num_val = 0
        word = lexer.nextWord()

        while (word != None):
            word = word.upper()
            num_val = None
            try:
                num_val = float(word)
            except:
                pass

            if (word in self.dictionary):
                self.dictionary[word](self)  #somehow act on this
            elif num_val != None:
                self.stack.append(num_val)
            else:
                raise Exception("Unknown Word")
            #continue parsing
            word = self.lexer.nextWord()

if __name__ == "__main__":
    if (len(sys.argv) < 2):
        print("Usage: {} <string>".format(sys.argv[0]))
        sys.exit(0)

    terp = Scratch()
    terp.run(sys.argv[1])
