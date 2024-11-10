
OPTION CASEMAP:NONE

onePixel macro
;Za³aduj do ymm1 i ymm2 piksele
 vmovdqu ymm1, ymmword ptr[RDI]
 vmovdqu ymm2, ymmword ptr[RDI]
;Rozpakuj high bytes do ymm1 a low do ymm2, YMM11 == 0 co pozwala na zamianê bajtów na wordy
 vpunpckhbw	ymm1, ymm1, ymm11
 vpunpcklbw	ymm2, ymm2, ymm11
;Przeka¿ do ymm3 wartoœæ filtra dla konkretnego piksela
 vpbroadcastw ymm3, word ptr[RBX]
;Przemnó¿ piksel przez filtr
 vpmullw ymm1, ymm1, ymm3
 vpmullw ymm2, ymm2, ymm3
;Zapisz sumy do ymm4 i ymm5
 vpaddw ymm4,ymm4,ymm1
 vpaddw ymm5,ymm5,ymm2
;PrzejdŸ do kolejnego piksela i kolejnej wartoœci filtru
 add rbx, 2
 add rdi, 3
 endm

.code
;ASMFilter(unsigned char outData[] -> RSI, unsigned char data[] -> RDI, int imWidth -> R8, int i -> R9, short int filter[] -> RSP+40)

MyProc2 proc EXPORT
;Ustaw RSI jako wskaŸnik na dane wyjœciowe
 mov RSI, RCX
;Ustaw RDI jako wskaŸnik na dane wejœciowe
 mov RDI, RDX
;Wyzeruj ymm4 i ymm5
 vpxor ymm4, ymm4, ymm4
 vpxor ymm5, ymm5, ymm5
;Przesuñ wskaŸnik na dane wejœæiowe i wyjœciowe o indeks pikseli
 add rdi, r9
 add rsi, r9
;Za³aduj do RAX d³ugoœæ zdjêcia pomno¿on¹ przez 3 (poniewa¿ jeden piksel sk³ada siê z 3 bajtów)
 mov rax, 3
 mul r8
;Ustaw rdi na piksel znajduj¹cy siê na lewo i do góry od piksela wejœciowego
 sub rdi, rax
 sub rdi, 3
;Za³aduj do rbx pierwsz¹ liczbê filtra (lewy górny róg)
 mov rbx, qword ptr [rsp+40];
;Wykonaj 3 razy makro obliczaj¹ce wartoœæ piksel*filtr
 onePixel
 onePixel
 onePixel
;Przesuñ rdi na pocz¹tek kolejnego rzêdu
 sub rdi, 9
 add rdi, rax

 onePixel
 onePixel
 onePixel
 
 sub rdi, 9
 add rdi, rax
 onePixel
 onePixel
 onePixel

;Po³¹cz obliczone w ymm5 i ymm4 wartoœci sum dla kolejnych piksli
 vpackuswb ymm0, ymm5, ymm4
;Zapisz piksel do tablicy wyjœciowej
 vmovdqu ymmword ptr[RSI+3], ymm0
 
 ret ; return z in EAX register
MyProc2 endp
end ; End of ASM file