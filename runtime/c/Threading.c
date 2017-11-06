#include <pthread.h>

// Gets the 'PTHREAD_MUTEX_ERRORCHECK' type.
int pthread_get_mutex_errorcheck_type()
{
    return PTHREAD_MUTEX_ERRORCHECK;
}

// Gets the size of a 'pthread_mutex_t' structure.
unsigned int pthread_get_mutex_size()
{
    return sizeof(pthread_mutex_t);
}

// Gets the size of a 'pthread_mutexattr_t' structure.
unsigned int pthread_get_mutexattr_size()
{
    return sizeof(pthread_mutexattr_t);
}

// Gets the size of a 'pthread_t' structure.
unsigned int pthread_get_thread_size()
{
    return sizeof(pthread_t);
}
